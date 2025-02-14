using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Search.Filtering;
using Litium.Accelerator.ViewModels.Search;
using Litium.FieldFramework;
using Litium.Globalization;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Runtime.DependencyInjection;
using Litium.Search;
using Litium.Security;
using Litium.Web;
using Litium.Web.Models.Globalization;
using Nest;

namespace Litium.Accelerator.Search.Searching
{
    [ServiceDecorator(typeof(FilterAggregator))]
    internal class FilterAggregatorDecorator : FilterAggregator
    {
        private const int MaximumCountOfFilters = 100;

        private readonly FilterAggregator _parent;
        private readonly SearchClientService _searchClientService;
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly ProductSearchServiceDecorator.SearchQueryBuilder _searchQueryBuilder;
        private readonly FieldDefinitionService _fieldDefinitionService;
        private readonly Lazy<MarketModel> _marketModel;
        private readonly Lazy<Guid> _assortmentSystemId;
        private readonly Lazy<Guid> _countrySystemId;
        private readonly Lazy<SearchPriceFilterService.Container> _priceContainer;

        public FilterAggregatorDecorator(
            FilterAggregator parent,
            CategoryService categoryService,
            CurrencyService currencyService,
            UrlService urlService,
            SearchClientService searchClientService,
            RequestModelAccessor requestModelAccessor,
            ProductSearchServiceDecorator.SearchQueryBuilder searchQueryBuilder,
            FieldDefinitionService fieldDefinitionService,
            SearchPriceFilterService priceFilterService,
            AuthorizationService authorizationService)
            : base(categoryService, currencyService, urlService, authorizationService)
        {
            _parent = parent;
            _searchClientService = searchClientService;
            _requestModelAccessor = requestModelAccessor;
            _searchQueryBuilder = searchQueryBuilder;
            _fieldDefinitionService = fieldDefinitionService;
            _marketModel = new Lazy<MarketModel>(() =>
                _requestModelAccessor.RequestModel.ChannelModel?.Channel?.MarketSystemId?.MapTo<MarketModel>());
            _assortmentSystemId = new Lazy<Guid>(() => _marketModel.Value?.Market.AssortmentSystemId ?? Guid.Empty);
            _countrySystemId = new Lazy<Guid>(() => _requestModelAccessor.RequestModel.CountryModel.SystemId);
            _priceContainer = new Lazy<SearchPriceFilterService.Container>(() => priceFilterService.GetPrices());
        }

        public override async Task<IEnumerable<GroupFilter>> GetFilterAsync(SearchQuery searchQuery,
            IEnumerable<string> fieldNames)
        {
            var fieldNamesList = fieldNames as IList<string> ?? fieldNames.ToList();

            if (!_searchClientService.IsConfigured)
            {
                return await _parent.GetFilterAsync(searchQuery, fieldNames);
            }
            if (fieldNamesList.Count > 0
                && !fieldNamesList.All(fieldName =>
                    fieldName.Equals(FilteringConstants.FilterNews, StringComparison.OrdinalIgnoreCase)))
            {
                var noFilterFieldNames = new HashSet<string>(new[]
                {
                        FilteringConstants.FilterPrice,
                        FilteringConstants.FilterNews,
                        FilteringConstants.FilterProductCategories
                    }, StringComparer.OrdinalIgnoreCase);

                var result = await _searchClientService.SearchAsync<ProductDocument>(CultureInfo.CurrentCulture, descriptor => descriptor
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

                        var aggregationTagNames = fieldNames
                            .Where(fieldName => !noFilterFieldNames.Contains(fieldName));
                        if(aggregationTagNames.Any())
                        {
                            aggs.AddRange(BuildFieldAggregations(rootAgg, aggregationTagNames));
                            aggs.Add(BuildFieldAggregation(rootAgg, aggregationTagNames));
                        }

                        if (fieldNamesList.Any(fieldName => fieldName.Equals(FilteringConstants.FilterPrice, StringComparison.OrdinalIgnoreCase)))
                        {
                            aggs.Add(BuildPriceAggregation(rootAgg));
                        }

                        if (fieldNamesList.Any(fieldName => fieldName.Equals(FilteringConstants.FilterProductCategories, StringComparison.OrdinalIgnoreCase)))
                        {
                            aggs.Add(BuildCategoryAggregation(rootAgg));
                        }

                        return aggs.Aggregate((a, b) => a & b);
                    }));

                return CollectGroupFilter();

                IEnumerable<GroupFilter> CollectGroupFilter()
                {
                    foreach (var fieldName in fieldNames)
                    {
                        if (fieldName.Equals(FilteringConstants.FilterPrice, StringComparison.OrdinalIgnoreCase))
                        {
                            var filterGroup = CollectPriceFacet();
                            if (filterGroup is object)
                            {
                                yield return filterGroup;
                            }
                        }
                        else if (fieldName.Equals(FilteringConstants.FilterNews, StringComparison.OrdinalIgnoreCase))
                        {
                            var filterGroup = GetNewsTag(searchQuery);
                            if (filterGroup is object)
                            {
                                yield return filterGroup;
                            }
                        }
                        else if (fieldName.Equals(FilteringConstants.FilterProductCategories, StringComparison.OrdinalIgnoreCase))
                        {
                            var filterGroup = CollectCategoryFacet();
                            if (filterGroup is object)
                            {
                                yield return filterGroup;
                            }
                        }
                        else
                        {
                            var tag = CollectFieldFacet(fieldName);
                            if (tag is object)
                            {
                                yield return tag;
                            }
                        }
                    }
                }

                AggregationContainerDescriptor<ProductDocument> BuildCategoryAggregation(
                    AggregationContainerDescriptor<ProductDocument> selector)
                {
                    return selector
                        .Filter("$Categories", filterContainer => filterContainer
                            .Filter(filterSelector => filterSelector
                                .Bool(bq =>
                                {
                                    if (searchQuery.ContainsFilter(includeCategoryFilter:false))
                                    {
                                        return bq.Must(m =>
                                            _searchQueryBuilder.BuildQuery(
                                                m,
                                                searchQuery,
                                                tags: searchQuery.Tags,
                                                addPriceFilterTags: true,
                                                addNewsFilterTags: true,
                                                addCategoryFilterTags: false,
                                                addDefaultQuery: false)

                                        );
                                    }
                                    return bq.Must(x => x.Term(valueSelector => valueSelector
                                        .Field(a => a.ChannelSystemId)
                                        .Value(_requestModelAccessor.RequestModel.ChannelModel?.SystemId)
                                    ));
                                }))
                            .Aggregations(container =>
                                container
                                    .Nested("$Categories", nestedDescriptor => nestedDescriptor
                                        .Path(x => x.MainCategories)
                                        .Aggregations(fieldAggregation => fieldAggregation
                                            .Filter("filter", fieldFilter => fieldFilter
                                                .Filter(filter => filter
                                                    .Term(valueSelector => valueSelector
                                                        .Field(x => x.MainCategories[0].AssortmentSystemId)
                                                        .Value(_assortmentSystemId.Value)
                                                    )
                                                )
                                                .Aggregations(tags => tags
                                                    .Terms("tags", termSelector => termSelector
                                                        .Field(field => field.MainCategories[0].AssortmentSystemId)
                                                        .Aggregations(subAggregation => subAggregation
                                                            .Terms("tag", valueSelector => valueSelector
                                                                .Field(x => x.MainCategories[0].CategorySystemId)
                                                                .Size(MaximumCountOfFilters)
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                            ));
                }

                IEnumerable<AggregationContainerDescriptor<ProductDocument>> BuildFieldAggregations(
                    AggregationContainerDescriptor<ProductDocument> selector, IEnumerable<string> fieldNames)
                {
                    return fieldNames
                        .Select(fieldName =>
                        {
                            if (searchQuery.ContainsFilter(exceptTag: fieldName))
                            {
                                return selector
                                    .Filter("facet_" + fieldName, filterContainer => filterContainer
                                        .Filter(filterSelector => filterSelector
                                            .Bool(bq => bq
                                                .Must(m => _searchQueryBuilder.BuildQuery(
                                                    m,
                                                    searchQuery,
                                                    tags: FilterFields(fieldName),
                                                    addPriceFilterTags: true,
                                                    addNewsFilterTags: true,
                                                    addCategoryFilterTags: true,
                                                    addDefaultQuery: false)
                                                )
                                            )
                                        )
                                        .Aggregations(x => BuildFilterAggregation(x, fieldName))
                                    );
                            }

                            return BuildFilterAggregation(selector, fieldName);
                        });

                    AggregationContainerDescriptor<ProductDocument> BuildFilterAggregation(AggregationContainerDescriptor<ProductDocument> container, string fieldName)
                    {
                        return container
                            .Nested("facet_" + fieldName, nestedPerField => nestedPerField
                                .Path(x => x.Tags)
                                .Aggregations(fieldAggregation => fieldAggregation
                                    .Filter("filter", fieldFilter => fieldFilter
                                        .Filter(filter => filter
                                            .Term(filterTerm => filterTerm
                                                .Field(field => field.Tags[0].Key)
                                                .Value(fieldName)
                                            )
                                        )
                                        .Aggregations(tags => tags
                                            .Terms("tags", termSelector => termSelector
                                                .Field(field => field.Tags[0].Key)
                                                .Aggregations(subAggregation => subAggregation
                                                    .Terms("tag", tag => tag
                                                        .Field(x => x.Tags[0].Value)
                                                        .Size(MaximumCountOfFilters)
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            );
                    }

                    IDictionary<string, ISet<string>> FilterFields(string fieldName)
                    {
                        return searchQuery
                            .Tags
                            .Where(x => !x.Key.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                            .ToDictionary(x => x.Key, x => x.Value);
                    }
                }

                AggregationContainerDescriptor<ProductDocument> BuildFieldAggregation(AggregationContainerDescriptor<ProductDocument> selector, IEnumerable<string> fieldNames)
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
                                                    .Terms(fieldNames)
                                                )
                                            )
                                        )
                                    )
                                    .Aggregations(termAgg => termAgg
                                        .Terms("tags", termSelector => termSelector
                                            .Field(x => x.Tags[0].Key)
                                            .Size(MaximumCountOfFilters)
                                            .Aggregations(subAggregation => subAggregation
                                                .Terms("tag", valueSelector => valueSelector
                                                    .Field(x => x.Tags[0].Value)
                                                    .Size(MaximumCountOfFilters)
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        );
                }

                AggregationContainerDescriptor<ProductDocument> BuildPriceAggregation(AggregationContainerDescriptor<ProductDocument> selector)
                {
                    if (searchQuery.ContainsFilter(includePriceFilter: false))
                    {
                        return selector
                                .Filter("$Prices", filterContainer => filterContainer
                                    .Filter(filterSelector => filterSelector
                                        .Bool(bq => bq
                                            .Must(m => _searchQueryBuilder.BuildQuery(
                                                    m,
                                                    searchQuery,
                                                    tags: searchQuery.Tags,
                                                    addPriceFilterTags: false,
                                                    addNewsFilterTags: true,
                                                    addCategoryFilterTags: true,
                                                    addDefaultQuery: false)
                                            )
                                        )
                                    )
                                    .Aggregations(x => BuildPriceAggregationItem(x))
                                );
                    }
                    else
                    {
                        return BuildPriceAggregationItem(selector);
                    };

                    AggregationContainerDescriptor<ProductDocument> BuildPriceAggregationItem(AggregationContainerDescriptor<ProductDocument> selector)
                    {
                        var priceType =
                            _requestModelAccessor.RequestModel.Cart.ShowPricesWithVat
                                ? "item.priceIncludeVat"
                                : "item.priceExcludeVat";
                        var scriptStr =
                            $"double r; for (item in params._source.prices) {{ boolean a = false; for (x in item.priceListSystemIds){{ a = params.id.contains(x); if( a == true) {{ break; }}}}  if ( a == true && item.countrySystemId == params.country && item.isCampaignPrice == false) {{ r = r == 0 ? {priceType} : Math.min(r, {priceType})}}}} return r";
                        return selector
                            .Terms("$Prices", prices => prices
                                .Script(script => script
                                    .Source(scriptStr)
                                    .Params(new Dictionary<string, object> {
                                            {"id", _priceContainer.Value.PriceLists.ToArray() },
                                            {"country", _countrySystemId.Value },
                                    }))
                                .Size(10000));
                    }
                }

                GroupFilter CollectCategoryFacet()
                {
                    var categoryBucket = result.Aggregations
                        .Global("$Categories")?
                        .Global("$Categories")?
                        .Filter("filter")?
                        .Terms("tags")?
                        .Buckets
                        .FirstOrDefault()?
                        .Terms("tag")?
                        .Buckets
                        .Select(x => new { CanConvert = Guid.TryParse(x.Key, out var id), Key = id, x.DocCount })
                        .Where(x => x.CanConvert)
                        .ToDictionary(x => x.Key, x => unchecked((int)x.DocCount));

                    if (categoryBucket == null)
                    {
                        return null;
                    }

                    return GetProductCategoryTag(searchQuery, categoryBucket);
                }

                GroupFilter CollectFieldFacet(string fieldName)
                {
                    var fieldDefinition = _fieldDefinitionService.Get<ProductArea>(fieldName);
                    if (fieldDefinition == null)
                    {
                        return null;
                    }

                    var allBuckets = result.Aggregations
                        .Global("$all-tags")?
                        .Filter("filter")?
                        .Terms("tags")?
                        .Buckets
                        .FirstOrDefault(x => x.Key.Equals(fieldName, StringComparison.OrdinalIgnoreCase))?
                        .Terms("tag")?
                        .Buckets;

                    if (allBuckets == null)
                    {
                        return null;
                    }

                    var topNode = result.Aggregations.Filter("facet_" + fieldName);
                    var tagBuckets = (topNode?.Nested("facet_" + fieldName) ?? topNode)?
                        .Filter("filter")?
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

                    return GetFilterTag(searchQuery, fieldDefinition, tagValues);
                }

                GroupFilter CollectPriceFacet()
                {
                    var priceBucket = result.Aggregations?.Filter("$Prices")?.Terms("$Prices")?.Buckets ?? result.Aggregations.Terms("$Prices")?.Buckets;
                    if (priceBucket != null)
                    {
                        var priceFacets = priceBucket.ToDictionary(x => decimal.Parse(x.Key, NumberStyles.Any, CultureInfo.InvariantCulture), x => unchecked((int)x.DocCount.GetValueOrDefault()));
                        var keys = priceFacets.Keys.Where(x => x > decimal.Zero).ToArray();
                        var minPrice = keys.Length > 0 ? (int)Math.Abs(keys.Min()) : 0;
                        var maxPrice = keys.Length > 0 ? (int)Math.Floor(keys.Max()) : 0;

                        var priceHits = GetPriceGroups(priceFacets, minPrice, maxPrice).ToList();
                        return GetPriceTag(searchQuery, priceHits, true, _requestModelAccessor.RequestModel.CountryModel.Country.CurrencySystemId);
                    }
                    return null;
                }
            }

            return Array.Empty<GroupFilter>();
        }

        protected override string NormalizeTag(string text)
        {
            return text;
        }
    }
}
