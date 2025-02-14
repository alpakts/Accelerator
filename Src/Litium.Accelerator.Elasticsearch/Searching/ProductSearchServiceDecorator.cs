using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Utilities;
using Litium.Accelerator.ViewModels.Brand;
using Litium.Accelerator.ViewModels.Search;
using Litium.Data.Queryable;
using Litium.FieldFramework;
using Litium.FieldFramework.FieldTypes;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Runtime.DependencyInjection;
using Litium.Search;
using Litium.Web;
using Litium.Web.Customers.TargetGroups;
using Litium.Web.Models.Globalization;
using Litium.Web.Models.Products;
using Nest;

namespace Litium.Accelerator.Search.Searching
{
    [ServiceDecorator(typeof(ProductSearchService))]
    internal class ProductSearchServiceDecorator : ProductSearchService
    {
        private const int MaximumCountOfBrands = 100;

        private readonly ProductSearchService _parent;
        private readonly SearchClientService _searchClientService;
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly SearchResultTransformationService _searchResultTransformationService;
        private readonly SearchQueryBuilder _searchQueryBuilder;
        private readonly FieldDefinitionService _fieldDefinitionService;

        public ProductSearchServiceDecorator(
            ProductSearchService parent,
            SearchClientService searchClientService,
            RequestModelAccessor requestModelAccessor,
            SearchResultTransformationService searchResultTransformationService,
            SearchQueryBuilder searchQueryBuilder,
            TargetGroupEngine targetGroupEngine,
            FieldDefinitionService fieldDefinitionService)
            : base(targetGroupEngine)
        {
            _parent = parent;
            _searchClientService = searchClientService;
            _requestModelAccessor = requestModelAccessor;
            _searchResultTransformationService = searchResultTransformationService;
            _searchQueryBuilder = searchQueryBuilder;
            _fieldDefinitionService = fieldDefinitionService;
        }

        public override async Task<SearchResult> SearchAsync(SearchQuery searchQuery, IDictionary<string, ISet<string>> tags = null, bool addPriceFilterTags = false, bool addNewsFilterTags = false, bool addCategoryFilterTags = false)
        {
            if (!_searchClientService.IsConfigured)
            {
                return await _parent.SearchAsync(
                    searchQuery,
                    tags: tags,
                    addPriceFilterTags: addPriceFilterTags,
                    addNewsFilterTags: addNewsFilterTags,
                    addCategoryFilterTags: addCategoryFilterTags);
            }

            if (string.IsNullOrEmpty(searchQuery.Text) && searchQuery.CategorySystemId == Guid.Empty && searchQuery.ProductListSystemId == null)
            {
                return null;
            }

            var response = await _searchClientService
                .SearchAsync<ProductDocument>(CultureInfo.CurrentCulture, selector => selector
                     .Skip((searchQuery.PageNumber - 1) * searchQuery.PageSize)
                     .Size(searchQuery.PageSize)
                     .QueryWithPermission(queryContainerDescriptor => _searchQueryBuilder.BuildQuery(
                         queryContainerDescriptor,
                         searchQuery,
                         tags,
                         addPriceFilterTags,
                         addNewsFilterTags,
                         addCategoryFilterTags,
                         true))
                     .Source(s => s
                        .Includes(i => i
                            .Fields(
                                f => f.IsBaseProduct,
                                f => f.BaseProductSystemId,
                                f => f.VariantSystemIds
                            )
                        )
                     )
                     .Sort(sortDescriptor => _searchQueryBuilder.BuildSorting(
                         sortDescriptor,
                         searchQuery))
                );

            return Transform(searchQuery, unchecked((int)response.Total), () =>
            {
                return _searchResultTransformationService.BuildSearchResultItems(
                    searchQuery,
                    response,
                    _requestModelAccessor.RequestModel.ChannelModel.SystemId);
            });
        }

        public override async Task<List<TagTerms>> GetTagTermsAsync(SearchQuery searchQuery, IEnumerable<string> tagNames)
        {
            if (!_searchClientService.IsConfigured)
            {
                return await _parent.GetTagTermsAsync(searchQuery, tagNames);
            }

            var searchResponse = await _searchClientService.SearchAsync<ProductDocument>(CultureInfo.CurrentCulture, descriptor => descriptor
                                                     .Size(0)
                                                     .QueryWithPermission(queryContainerDescriptor => _searchQueryBuilder.BuildQuery(queryContainerDescriptor,
                                                                                                                                     searchQuery,
                                                                                                                                     tags: null,
                                                                                                                                     addPriceFilterTags: false,
                                                                                                                                     addNewsFilterTags: false,
                                                                                                                                     addCategoryFilterTags: false,
                                                                                                                                     addDefaultQuery: true))
                                                     .Aggregations(rootAgg =>
                                                     {
                                                         var aggs = new List<AggregationContainerDescriptor<ProductDocument>>();
                                                         aggs.AddRange(tagNames.Select(tagName => BuildFilterAggregation(rootAgg, tagName)));
                                                         aggs.Add(BuildTagAggregation(rootAgg, tagNames));

                                                         return aggs.Aggregate((a, b) => a & b);
                                                     }));

            var result = new List<TagTerms>();
            foreach (var tagName in tagNames)
            {
                var tag = CollectTagTerms(tagName);
                if (tag != null)
                {
                    result.Add(tag);
                }
            }

            return result;

            AggregationContainerDescriptor<ProductDocument> BuildFilterAggregation(AggregationContainerDescriptor<ProductDocument> container, string tagName)
            {
                return container
                       .Nested(tagName, nestedPerTag => nestedPerTag
                           .Path(x => x.Tags)
                           .Aggregations(tagAggregation => tagAggregation
                               .Filter("filter", tagFilter => tagFilter
                                   .Filter(filter => filter
                                       .Term(filterTerm => filterTerm
                                           .Field(field => field.Tags[0].Key)
                                           .Value(tagName)
                                       )
                                   )
                                   .Aggregations(tags => tags
                                       .Terms("tags", termSelector => termSelector
                                           .Field(field => field.Tags[0].Key)
                                           .Aggregations(subAggregation => subAggregation
                                               .Terms("tag", tag => tag
                                                   .Field(x => x.Tags[0].Value)
                                                   .Size(MaximumCountOfBrands)
                                               )
                                           )
                                       )
                                   )
                               )
                           )
                       );
            }

            AggregationContainerDescriptor<ProductDocument> BuildTagAggregation(AggregationContainerDescriptor<ProductDocument> selector, IEnumerable<string> tagNames)
            {
                return selector
                       .Nested("$all-tags", filterContainer => filterContainer
                           .Path(x => x.Tags)
                           .Aggregations(a => a
                               .Filter("filter", filterSelector => filterSelector
                                   .Filter(ff => ff
                                       .Bool(bq => bq
                                           .Must(m => m
                                               .Terms(t => t
                                                   .Field(x => x.Tags[0].Key)
                                                   .Terms(tagNames)
                                               )
                                           )
                                       )
                                   )
                                   .Aggregations(termAgg => termAgg
                                       .Terms("tags", termSelector => termSelector
                                           .Field(x => x.Tags[0].Key)
                                           .Aggregations(subAggregation => subAggregation
                                               .Terms("tag", valueSelector => valueSelector
                                                   .Field(x => x.Tags[0].Value)
                                                   .Size(MaximumCountOfBrands)
                                               )
                                           )
                                       )
                                   )
                               )
                           )
                       );
            }

            TagTerms CollectTagTerms(string tagName)
            {
                var fieldDefinition = _fieldDefinitionService.Get<ProductArea>(tagName);
                if (fieldDefinition == null)
                {
                    return null;
                }

                var allBuckets = searchResponse.Aggregations
                                               .Global("$all-tags")?
                                               .Filter("filter")?
                                               .Terms("tags")?
                                               .Buckets
                                               .FirstOrDefault(x => x.Key.Equals(fieldDefinition.Id, StringComparison.OrdinalIgnoreCase))?
                                               .Terms("tag")?
                                               .Buckets;

                if (allBuckets == null)
                {
                    return null;
                }

                var topNode = searchResponse.Aggregations.Filter(fieldDefinition.Id);
                var tagBuckets = (topNode?.Nested(fieldDefinition.Id) ?? topNode)?.Filter("filter")?
                                                                                  .Terms("tags")?
                                                                                  .Buckets
                                                                                  .FirstOrDefault()?
                                                                                  .Terms("tag")?
                                                                                  .Buckets;

                var tagValues = new Dictionary<string, int>();
                foreach (var item in allBuckets)
                {
                    var current = tagBuckets?.FirstOrDefault(x => x.Key.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                    tagValues.Add(item.Key, unchecked((int)(current?.DocCount ?? 0)));
                }

                return new TagTerms
                {
                    TagName = fieldDefinition.Localizations.CurrentCulture.Name ?? fieldDefinition.Id,
                    TermCounts = tagValues
                        .Select(x =>
                        {
                            string key;
                            switch (fieldDefinition.FieldType)
                            {
                                case SystemFieldTypeConstants.Decimal:
                                case SystemFieldTypeConstants.Int:
                                    {
                                        key = x.Key.TrimStart('0');
                                        break;
                                    }
                                case SystemFieldTypeConstants.Date:
                                case SystemFieldTypeConstants.DateTime:
                                    {
                                        if (long.TryParse(x.Key, NumberStyles.Any, CultureInfo.InvariantCulture, out var l))
                                        {
                                            key = new DateTime(l).ToShortDateString();
                                        }
                                        else
                                        {
                                            goto default;
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        key = x.Key;
                                        break;
                                    }
                            }

                            return new TermCount
                            {
                                Term = key,
                                Count = x.Value
                            };
                        })
                        .ToList()
                };
            }
        }

        [Service(ServiceType = typeof(SearchResultTransformationService), Lifetime = DependencyLifetime.Singleton)]
        internal class SearchResultTransformationService
        {
            private readonly BaseProductService _baseProductService;
            private readonly VariantService _variantService;
            private readonly FieldDefinitionService _fieldDefinitionService;
            private readonly ProductModelBuilder _productModelBuilder;
            private readonly UrlService _urlService;
            private readonly CategoryService _categoryService;

            public SearchResultTransformationService(
                BaseProductService baseProductService,
                VariantService variantService,
                FieldDefinitionService fieldDefinitionService,
                ProductModelBuilder productModelBuilder,
                UrlService urlService,
                CategoryService categoryService)
            {
                _baseProductService = baseProductService;
                _variantService = variantService;
                _fieldDefinitionService = fieldDefinitionService;
                _productModelBuilder = productModelBuilder;
                _urlService = urlService;
                _categoryService = categoryService;
            }

            public IEnumerable<SearchResultItem> BuildSearchResultItems(SearchQuery searchQuery, ISearchResponse<ProductDocument> searchResponse, Guid channelSystemId)
            {
                var products = CreateProductModel(searchResponse.Hits, searchQuery, channelSystemId);

                return products
                    .Select(x => new ProductSearchResult
                    {
                        Item = x,
                        Id = x.SelectedVariant.SystemId,
                        Name = x.GetValue<string>(SystemFieldDefinitionConstants.Name),
                        Url = x.UseVariantUrl
                            ? _urlService.GetUrl(x.SelectedVariant, new ProductUrlArgs(channelSystemId))
                            : _urlService.GetUrl(x.BaseProduct, new ProductUrlArgs(channelSystemId))
                    }).ToList();
            }

            private IEnumerable<ProductModel> CreateProductModel(IReadOnlyCollection<IHit<ProductDocument>> hits, SearchQuery searchQuery, Guid channelSystemId)
            {
                if (hits.Count == 0)
                {
                    yield break;
                }

                foreach (var hit in hits)
                {
                    ProductModel model = null;

                    var item = hit.Source;
                    var variants = _variantService.Get(item.VariantSystemIds)
                        .Where(x => !string.IsNullOrEmpty(_urlService.GetUrl(x, new ProductUrlArgs(channelSystemId))))
                        .OrderBy(x => x.SortIndex)
                        .ToList();

                    if (variants.Count == 0)
                    {
                        continue;
                    }

                    if (item.IsBaseProduct)
                    {
                        var baseProduct = _baseProductService.Get(item.BaseProductSystemId);
                        if (baseProduct == null)
                        {
                            continue;
                        }

                        model = CreateProductModel(searchQuery, baseProduct, variants, channelSystemId);
                    }
                    else if (variants.Count > 1)
                    {
                        model = CreateProductModel(searchQuery, null, variants, channelSystemId);
                    }
                    else
                    {
                        model = _productModelBuilder.BuildFromVariant(variants[0]);
                    }

                    if (model != null)
                    {
                        yield return model;
                    }
                }
            }

            private ProductModel CreateProductModel(SearchQuery searchQuery, BaseProduct baseProduct, ICollection<Variant> variants, Guid channelSystemId)
            {
                IEnumerable<Variant> currentVariants = variants;
                if (searchQuery.CategorySystemId != null && searchQuery.CategorySystemId != Guid.Empty)
                {
                    var product = baseProduct ?? _baseProductService.Get(currentVariants.First().BaseProductSystemId);
                    var categoryLink = _categoryService.Get(searchQuery.CategorySystemId.Value)?.ProductLinks.FirstOrDefault(x => x.BaseProductSystemId == product.SystemId);
                    if (categoryLink != null)
                    {
                        currentVariants = currentVariants.Where(x => categoryLink.ActiveVariantSystemIds.Contains(x.SystemId));
                    }
                }

                currentVariants = currentVariants
                    .Where(x => x.IsActive(channelSystemId))
                    .OrderBy(x => x.SortIndex);

                if (searchQuery.Tags.Count > 0)
                {
                    var order = new ConcurrentDictionary<Variant, int>();
                    Variant firstVariant = null;
                    foreach (var tag in searchQuery.Tags)
                    {
                        var fieldDefinition = _fieldDefinitionService.Get<ProductArea>(tag.Key);
                        // ReSharper disable once PossibleMultipleEnumeration
                        foreach (var variant in currentVariants)
                        {
                            if (firstVariant == null)
                            {
                                firstVariant = variant;
                            }

                            CalculateTagRelevance(order, tag, fieldDefinition, variant);
                        }
                    }

                    if (!order.IsEmpty)
                    {
                        currentVariants = order.OrderByDescending(x => x.Value).Select(x => x.Key);
                    }
                }

                if (!currentVariants.Any())
                {
                    return null;
                }

                return baseProduct == null
                    ? _productModelBuilder.BuildFromVariant(currentVariants.First())
                    : _productModelBuilder.BuildFromBaseProduct(baseProduct, currentVariants.First());

                static void CalculateTagRelevance(ConcurrentDictionary<Variant, int> order, KeyValuePair<string, ISet<string>> tag, FieldDefinition fieldDefinition, Variant variant)
                {
                    var variantValue = variant.Fields[tag.Key, CultureInfo.CurrentCulture] ?? variant.Fields[tag.Key];
                    if (!(variantValue is string) && variantValue is IEnumerable enumVariantValue)
                    {
                        foreach (var item in enumVariantValue)
                        {
                            var value = GetTranslatedValue(item, CultureInfo.CurrentCulture, fieldDefinition);
                            if (tag.Value.Contains(value))
                            {
                                order.AddOrUpdate(variant, _ => 1, (_, c) => c + 1);
                            }
                        }
                    }
                    else
                    {
                        var value = GetTranslatedValue(variantValue, CultureInfo.CurrentCulture, fieldDefinition);
                        if (tag.Value.Contains(value))
                        {
                            order.AddOrUpdate(variant, _ => 1, (_, c) => c + 1);
                        }
                    }

                    static string GetTranslatedValue(object value, CultureInfo cultureInfo, FieldDefinition fieldDefinition)
                    {
                        if (fieldDefinition == null || value is null)
                        {
                            return value as string;
                        }

                        switch (fieldDefinition?.FieldType)
                        {
                            case SystemFieldTypeConstants.Date:
                            case SystemFieldTypeConstants.DateTime:
                                return (value as DateTimeOffset?)?.ToString(CultureInfo.InvariantCulture.DateTimeFormat) ?? value as string;
                            case SystemFieldTypeConstants.Decimal:
                                return (value as decimal?)?.ToString("0.########", CultureInfo.InvariantCulture.NumberFormat) ?? value as string;
                            case SystemFieldTypeConstants.DecimalOption:
                                {
                                    var option = fieldDefinition.Option as DecimalOption;
                                    var decimalValue = value as decimal?;

                                    var item = option?.Items.FirstOrDefault(x => x.Value == decimalValue);
                                    if (item != null && item.Name.TryGetValue(cultureInfo.Name, out var translation) && !string.IsNullOrEmpty(translation))
                                    {
                                        return translation;
                                    }
                                    goto case SystemFieldTypeConstants.Decimal;
                                }
                            case SystemFieldTypeConstants.Int:
                                return (value as int?)?.ToString(CultureInfo.InvariantCulture.NumberFormat) ?? value as string;
                            case SystemFieldTypeConstants.IntOption:
                                {
                                    var option = fieldDefinition.Option as IntOption;
                                    var intValue = value as int?;

                                    var item = option?.Items.FirstOrDefault(x => x.Value == intValue);
                                    if (item != null && item.Name.TryGetValue(cultureInfo.Name, out var translation) && !string.IsNullOrEmpty(translation))
                                    {
                                        return translation;
                                    }
                                    goto case SystemFieldTypeConstants.Int;
                                }
                            case SystemFieldTypeConstants.Long:
                                return (value as long?)?.ToString(CultureInfo.InvariantCulture.NumberFormat) ?? value as string;
                            case SystemFieldTypeConstants.TextOption:
                                {
                                    var option = fieldDefinition.Option as TextOption;
                                    var stringValue = value as string;

                                    var item = option?.Items.FirstOrDefault(x => x.Value == stringValue);
                                    if (item != null && item.Name.TryGetValue(cultureInfo.Name, out var translation) && !string.IsNullOrEmpty(translation))
                                    {
                                        return translation;
                                    }
                                    goto default;
                                }
                            default:
                                return value as string;
                        }
                    }
                }
            }
        }

        [Service(ServiceType = typeof(SearchQueryBuilder), Lifetime = DependencyLifetime.Scoped)]
        internal class SearchQueryBuilder
        {
            private readonly RequestModelAccessor _requestModelAccessor;
            private readonly SearchPriceFilterService _priceFilterService;
            private readonly PersonStorage _personStorage;

            private readonly Lazy<MarketModel> _marketModel;
            private readonly Lazy<Guid> _countrySystemId;
            private readonly Lazy<Guid> _assortmentSystemId;
            private readonly Lazy<SearchPriceFilterService.Container> _priceContainer;

            public SearchQueryBuilder(
                RequestModelAccessor requestModelAccessor,
                SearchPriceFilterService priceFilterService,
                PersonStorage personStorage)
            {
                _requestModelAccessor = requestModelAccessor;
                _priceFilterService = priceFilterService;
                _personStorage = personStorage;

                _marketModel = new Lazy<MarketModel>(() => _requestModelAccessor.RequestModel.ChannelModel?.Channel?.MarketSystemId?.MapTo<MarketModel>());
                _countrySystemId = new Lazy<Guid>(() => _requestModelAccessor.RequestModel.CountryModel.SystemId);
                _assortmentSystemId = new Lazy<Guid>(() => _marketModel.Value?.Market.AssortmentSystemId ?? Guid.Empty);
                _priceContainer = new Lazy<SearchPriceFilterService.Container>(() => _priceFilterService.GetPrices());
            }

            public QueryContainer BuildQuery(
                QueryContainerDescriptor<ProductDocument> qc,
                SearchQuery searchQuery,
                IDictionary<string, ISet<string>> tags = null,
                bool addPriceFilterTags = false,
                bool addNewsFilterTags = false,
                bool addCategoryFilterTags = false,
                bool addDefaultQuery = true)
            {
                var allQueries = new List<QueryContainer>();

                if (addDefaultQuery)
                {
                    allQueries.Add(qc.PublishedOnChannel());
                    allQueries.Add(qc.Bool(b => b.Filter(bf => bf.Term(t => t.Field(x => x.Assortments).Value(_assortmentSystemId.Value)))));
                    if (_personStorage.CurrentSelectedOrganizationSystemId is Guid selectedOrganizationSystemId)
                    {
                        allQueries.Add((qc.Bool(b => b.Filter(bf => bf.Term(t => t.Field(x => x.Organizations).Value(Guid.Empty))))
                                        || qc.Bool(b => b.Filter(bf => bf.Term(t => t.Field(x => x.Organizations).Value(selectedOrganizationSystemId))))));
                    }
                    else
                    {
                        allQueries.Add(qc.Bool(b => b.Filter(bf => bf.Term(t => t.Field(x => x.Organizations).Value(Guid.Empty)))));
                    }

                    if (searchQuery.ProductListSystemId == null)
                    {
                        if (searchQuery.CategorySystemId != null)
                        {
                            if (searchQuery.CategoryShowRecursively)
                            {
                                allQueries.Add(qc.Bool(b => b.Filter(bf => bf.Term(t => t.Field(x => x.ParentCategories).Value(searchQuery.CategorySystemId.Value)))));
                            }
                            else
                            {
                                allQueries.Add(qc.Bool(b => b.Filter(bf => bf.Term(t => t.Field(x => x.Categories).Value(searchQuery.CategorySystemId.Value)))));
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(searchQuery.Text))
                        {
                            allQueries.Add((qc.Match(x => x.Field(z => z.Name).Query(searchQuery.Text).Fuzziness(Fuzziness.Auto).Boost(10).SynonymAnalyzer())
                                            || qc.Match(x => x.Field(z => z.Name.Suffix("ngram")).Query(searchQuery.Text).Boost(20).SynonymAnalyzer())
                                            || qc.Match(x => x.Field(z => z.ArticleNumber).Query(searchQuery.Text.ToLower()).Boost(2))
                                            || qc.Match(x => x.Field(z => z.Content).Query(searchQuery.Text).Fuzziness(Fuzziness.Auto).SynonymAnalyzer())));
                        }
                    }
                    else
                    {
                        allQueries.Add(qc.Bool(b => b.Filter(bf => bf.Term(t => t.Field(x => x.ProductLists).Value(searchQuery.ProductListSystemId.Value)))));
                    }

                    if (searchQuery.ArticleNumbers?.Any() == true)
                    {
                        allQueries.Add(qc.Terms(terms => terms.Field(x => x.ArticleNumber).Terms(searchQuery.ArticleNumbers)));
                    }

                    var pageModel = _requestModelAccessor.RequestModel.CurrentPageModel;
                    if (pageModel.IsBrandPageType())
                    {
                        if (tags != null)
                        {
                            if (!tags.ContainsKey(BrandListViewModel.TagName))
                            {
                                tags.Add(BrandListViewModel.TagName, new SortedSet<string>(new[] { pageModel.Page.Localizations.CurrentCulture.Name }));
                            }
                        }
                        else
                        {
                            tags = new Dictionary<string, ISet<string>> { { BrandListViewModel.TagName, new SortedSet<string>(new[] { pageModel.Page.Localizations.CurrentCulture.Name }) } };
                        }
                    }
                }

                if (tags != null)
                {
                    foreach (var tag in tags.Where(x => x.Value.Count > 0))
                    {
                        var filterTags = tag.Value
                            .Select<string, Func<QueryContainerDescriptor<ProductDocument>, QueryContainer>>(tagValue =>
                               s => s
                                .Nested(n => n
                                    .Path(x => x.Tags)
                                    .Query(nq
                                        => nq.Term(t => t.Field(f => f.Tags[0].Key).Value(tag.Key))
                                        && nq.Term(t => t.Field(f => f.Tags[0].Value).Value(tagValue))
                                    )
                                ));
                        allQueries.Add(qc.Bool(b => b.Filter(bf => bf.Bool(bb => bb.Should(filterTags)))));
                    }
                }

                if (addCategoryFilterTags)
                {
                    if (searchQuery.Category.Count > 0)
                    {
                        allQueries.Add(qc.Bool(b => b.Filter(bf => bf.Bool(bb => bb.Should(searchQuery.Category
                               .Select<Guid, Func<QueryContainerDescriptor<ProductDocument>, QueryContainer>>(x =>
                                   s => s.Term(t => t.Field(f => f.Categories).Value(x))))))));
                    }
                }

                if (addPriceFilterTags)
                {
                    var priceFilters = _priceFilterService
                        .GetPriceFilterTags(searchQuery, _priceContainer.Value, _countrySystemId.Value)
                        .ToList();

                    if (priceFilters.Count > 0)
                    {
                        allQueries.Add(qc.Bool(b => b.Filter(bf => bf.Bool(bb => bb.Should(priceFilters)))));
                    }
                }

                if (addNewsFilterTags)
                {
                    if (searchQuery.NewsDate != null)
                    {
                        allQueries.Add(qc.Bool(b => b.Filter(bf => bf.DateRange(r => r
                           .Field(x => x.NewsDate)
                           .GreaterThan(searchQuery.NewsDate.Item1)
                           .LessThan(searchQuery.NewsDate.Item2)))));
                    }
                }

                if (allQueries.Count == 0)
                {
                    return qc;
                }

                return allQueries.Aggregate((a, b) => a & b);
            }

            public IPromise<IList<ISort>> BuildSorting(Nest.SortDescriptor<ProductDocument> sortDescriptor, SearchQuery searchQuery)
            {
                var s = sortDescriptor;
                var isShowPriceWithVat = _requestModelAccessor.RequestModel.Cart.ShowPricesWithVat;
                switch (searchQuery.SortBy)
                {
                    case SearchQueryConstants.Price:
                        {
                            var priceFilters = _priceFilterService
                                .GetPriceFilterTags(searchQuery, _priceContainer.Value, _countrySystemId.Value, true)
                                .ToList();
                            s = s.Field(f => new FieldSort
                            {
                                Field = isShowPriceWithVat
                                    ? Infer.Field<ProductDocument>(ff => ff.Prices[0].PriceIncludeVat)
                                    : Infer.Field<ProductDocument>(ff => ff.Prices[0].PriceExcludeVat),
                                Order = searchQuery.SortDirection == SortDirection.Ascending ? SortOrder.Ascending : SortOrder.Descending,
                                Mode = SortMode.Min,
                                Nested = new NestedSort
                                {
                                    Path = Infer.Field<ProductDocument>(ff => ff.Prices),
                                    Filter = new QueryContainerDescriptor<ProductDocument>().Bool(bb => bb.Should(priceFilters))
                                }
                            });
                        }
                        break;

                    case SearchQueryConstants.Name:
                        {
                            // dont need any special sortings from the searchindex
                        }
                        break;

                    case SearchQueryConstants.News:
                        {
                            s = s.Descending(x => x.NewsDate);
                        }
                        break;

                    case SearchQueryConstants.Popular:
                        {
                            s = s.Field(f => new FieldSort
                            {
                                Field = Infer.Field<ProductDocument>(ff => ff.PurchaseHistoryQuantity),
                                Order = SortOrder.Descending,
                            });
                        }
                        break;

                    case SearchQueryConstants.Recommended:
                        {
                            if (searchQuery.ProductListSystemId != null)
                            {
                                var productListSystemId = searchQuery.ProductListSystemId.GetValueOrDefault();
                                s = s.Field(f => new FieldSort
                                {
                                    Field = Infer.Field<ProductDocument>(ff => ff.ProductListSortIndex[0].SortIndex),
                                    Order = searchQuery.SortDirection == SortDirection.Ascending ? SortOrder.Ascending : SortOrder.Descending,
                                    Nested = new NestedSort
                                    {
                                        Path = Infer.Field<ProductDocument>(ff => ff.ProductListSortIndex),
                                        Filter = new TermQuery
                                        {
                                            Field = Infer.Field<ProductDocument>(ff => ff.ProductListSortIndex[0].SystemId),
                                            Value = productListSystemId
                                        }
                                    }
                                });
                            }
                            else if (searchQuery.CategorySystemId != null)
                            {
                                var categorySystemId = searchQuery.CategorySystemId.GetValueOrDefault();
                                s = s.Field(f => new FieldSort
                                {
                                    Field = Infer.Field<ProductDocument>(ff => ff.CategorySortIndex[0].SortIndex),
                                    Order = searchQuery.SortDirection == SortDirection.Ascending ? SortOrder.Ascending : SortOrder.Descending,
                                    Nested = new NestedSort
                                    {
                                        Path = Infer.Field<ProductDocument>(ff => ff.CategorySortIndex),
                                        Filter = new TermQuery
                                        {
                                            Field = Infer.Field<ProductDocument>(ff => ff.CategorySortIndex[0].SystemId),
                                            Value = categorySystemId
                                        }
                                    }
                                });
                            }
                        }
                        break;
                    default:
                        {
                            if (!string.IsNullOrWhiteSpace(searchQuery.Text) || _requestModelAccessor.RequestModel.CurrentPageModel.IsSearchResultPageType())
                            {
                                // always sort products by their score, if no free-text is entered the score will be the same for all the products
                                s = s.Descending(SortSpecialField.Score);
                            }
                            else
                            {
                                if (searchQuery.Type == SearchType.Products)
                                {
                                    goto case SearchQueryConstants.Popular;
                                }
                                if (searchQuery.Type == SearchType.Category)
                                {
                                    goto case SearchQueryConstants.Recommended;
                                }
                            }
                            goto case SearchQueryConstants.Name;
                        }
                }

                // default sorting is to always sort products after their name, article number
                s = s
                    .Field(f => searchQuery.SortDirection == SortDirection.Ascending
                    ? f.Field(x => x.Name.Suffix("keyword")).Ascending()
                    : f.Field(x => x.Name.Suffix("keyword")).Descending())
                   .Ascending(x => x.ArticleNumber);
                return s;
            }
        }
    }
}
