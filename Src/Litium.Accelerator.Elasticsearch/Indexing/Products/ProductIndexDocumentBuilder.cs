using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Search.Indexing.PurchaseHistories;
using Litium.Accelerator.Search.Filtering;
using Litium.Accelerator.Services;
using Litium.FieldFramework;
using Litium.FieldFramework.FieldTypes;
using Litium.Globalization;
using Litium.Products;
using Litium.Search;
using Litium.Search.Indexing;
using Litium.Web;

namespace Litium.Accelerator.Search.Indexing.Products
{
    public class ProductIndexDocumentBuilder : MultilingualIndexDocumentBuilderBase<ProductDocument>
    {
        private readonly BaseProductService _baseProductService;
        private readonly FieldTemplateService _fieldTemplateService;
        private readonly VariantService _variantService;
        private readonly TemplateSettingService _templateSettingService;
        private readonly ProductIndexingContext _productIndexingContext;
        private readonly CategoryService _categoryService;
        private readonly CountryService _countryService;
        private readonly PriceListService _priceListService;
        private readonly PriceListItemService _priceListItemService;
        private readonly ProductListService _productListService;
        private readonly ProductListItemService _productListItemService;
        private readonly FilterService _filterService;
        private readonly FieldDefinitionService _fieldDefinitionService;
        private readonly SearchPermissionService _searchPermissionService;
        private readonly ContentBuilderService _contentBuilderService;
        private readonly ChannelService _channelService;
        private readonly LanguageService _languageService;
        private readonly UrlService _urlService;
        private readonly PurchaseHistoryService _purchaseHistoryService;

        public ProductIndexDocumentBuilder(
            IndexDocumentBuilderDependencies dependencies,
            BaseProductService baseProductService,
            FieldTemplateService fieldTemplateService,
            VariantService variantService,
            TemplateSettingService templateSettingService,
            ProductIndexingContext productIndexingContext,
            CategoryService categoryService,
            CountryService countryService,
            PriceListService priceListService,
            PriceListItemService priceListItemService,
            ProductListService productListService,
            ProductListItemService productListItemService,
            FilterService filterService,
            FieldDefinitionService fieldDefinitionService,
            SearchPermissionService searchPermissionService,
            ContentBuilderService contentBuilderService,
            ChannelService channelService,
            LanguageService languageService,
            UrlService urlService,
            PurchaseHistoryService purchaseHistoryService)
            : base(dependencies)
        {
            _baseProductService = baseProductService;
            _fieldTemplateService = fieldTemplateService;
            _variantService = variantService;
            _templateSettingService = templateSettingService;
            _productIndexingContext = productIndexingContext;
            _categoryService = categoryService;
            _countryService = countryService;
            _priceListService = priceListService;
            _priceListItemService = priceListItemService;
            _productListService = productListService;
            _productListItemService = productListItemService;
            _filterService = filterService;
            _fieldDefinitionService = fieldDefinitionService;
            _searchPermissionService = searchPermissionService;
            _contentBuilderService = contentBuilderService;
            _channelService = channelService;
            _languageService = languageService;
            _urlService = urlService;
            _purchaseHistoryService = purchaseHistoryService;
        }

        public override IEnumerable<(CultureInfo, IDocument)> BuildIndexDocuments(IndexQueueItem item)
        {
            var channels = _channelService.GetAll().Where(x => x.ProductLanguageSystemId.HasValue && x.ProductLanguageSystemId.Value != Guid.Empty).ToList();

            var baseProduct = _baseProductService.Get(item.SystemId);
            if (baseProduct == null)
            {
                // Product is removed.
                // BuildRemoveIndexDocuments will be invoked by the removing of product.
                yield break;
            }

            var productFieldTemplate = _fieldTemplateService.Get<ProductFieldTemplate>(baseProduct.FieldTemplateSystemId);
            if (productFieldTemplate == null)
            {
                // Field template is removed, all products with this template is also removed.
                // BuildRemoveIndexDocuments will be invoked by the removing of product.
                yield break;
            }

            var variants = _variantService.GetByBaseProduct(baseProduct.SystemId).ToList();
            if (variants.Count == 0)
            {
                // No variants exists for the BaseProduct, remove any existing document from index
                foreach (var channel in channels.GroupBy(x => x.ProductLanguageSystemId))
                {
                    var cultureInfo = _languageService.Get(channel.First().ProductLanguageSystemId.GetValueOrDefault())?.CultureInfo;
                    if (cultureInfo is null)
                    {
                        continue;
                    }
                    yield return (cultureInfo, RemoveByFieldDocument.Create<ProductDocument, Guid>(x => x.BaseProductSystemId, item.SystemId));
                }
                yield break;
            }

            var productDocuments = CreateModelsPerChannel(baseProduct, productFieldTemplate, ref variants);
            var usedVariantSystemIds = productDocuments.SelectMany(x => x.VariantSystemIds).ToHashSet();
            var usedChannelSystemIds = productDocuments.Select(x => x.ChannelSystemId).ToHashSet();

            var context = new Context(_contentBuilderService)
            {
                BaseProduct = baseProduct,
                UsedVariants = variants.Where(x => usedVariantSystemIds.Contains(x.SystemId)).ToList(),
                Permissions = _searchPermissionService.GetPermissions(baseProduct),
                FieldTemplate = productFieldTemplate,
                ChannelCountryLinks = channels.Where(x => usedChannelSystemIds.Contains(x.SystemId))
                    .ToDictionary(x => x.SystemId, x => x.CountryLinks.Select(s => s.CountrySystemId).ToHashSet())
            };
            context.Prices = BuildPriceData(context);
            context.PurchaseHistory = BuildPurchaseHistoryData(context);

            var articleNumberInChannel = new HashSet<(string ArticleNumber, Guid ChannelId)>();
            foreach (var model in productDocuments)
            {
                var currentVariants = variants.Where(x => model.VariantSystemIds.Contains(x.SystemId)).ToList();
                var channel = _channelService.Get(model.ChannelSystemId);
                if (channel is null)
                {
                    // Orphaned category link exists, skip to create new index document.
                    continue;
                }

                var cultureInfo = _languageService.Get(channel.ProductLanguageSystemId.GetValueOrDefault())?.CultureInfo;
                if (cultureInfo is null || currentVariants.Count == 0)
                {
                    // Channel does not have a culture or we don't have any variants in this channel.
                    continue;
                }

                PopulateProductDocument(model, cultureInfo, currentVariants, context, !productFieldTemplate.UseVariantUrl);
                articleNumberInChannel.Add((model.ArticleNumber, channel.SystemId));
                yield return (cultureInfo, model);
            }

            // Remove all documents for all channel-combinations that not is published
            foreach (var channel in channels)
            {
                var cultureInfo = _languageService.Get(channel.ProductLanguageSystemId.GetValueOrDefault())?.CultureInfo;
                if (cultureInfo is null)
                {
                    continue;
                }

                var channelNotUsed = !usedChannelSystemIds.Contains(channel.SystemId);
                if (channelNotUsed)
                {
                    yield return (cultureInfo, new RemoveDocument(new ProductDocument { ArticleNumber = baseProduct.Id, ChannelSystemId = channel.SystemId }));
                }

                foreach (var variant in variants.Where(x => channelNotUsed || !articleNumberInChannel.Contains((x.Id.ToLowerInvariant(), channel.SystemId))))
                {
                    yield return (cultureInfo, new RemoveDocument(new ProductDocument { ArticleNumber = variant.Id, ChannelSystemId = channel.SystemId }));
                }
            }
        }

        public override IEnumerable<IDocument> BuildRemoveIndexDocuments(IndexQueueItem item)
        {
            yield return RemoveByFieldDocument.Create<ProductDocument, Guid>(x => x.BaseProductSystemId, item.SystemId);
        }

        private IDictionary<Guid, IDictionary<Guid, decimal>> BuildPurchaseHistoryData(Context context)
        {
            return context.UsedVariants.ToDictionary(
                x => x.SystemId,
                variant => _purchaseHistoryService.TryGet(variant.Id, out var items) ? items : new Dictionary<Guid, decimal>());
        }

        private IDictionary<Guid, IList<ProductDocument.PriceItem>> BuildPriceData(Context context)
        {
            var countrySystemIds = context.ChannelCountryLinks.SelectMany(x => x.Value).ToHashSet();
            var countries = _countryService.Get(countrySystemIds).ToDictionary(x => x.SystemId, x => x.TaxClassLinks?.FirstOrDefault(x => x.TaxClassSystemId == context.BaseProduct.TaxClassSystemId)?.VatRate ?? x.StandardVatRate);
            return context.UsedVariants.ToDictionary(x => x.SystemId, x => (IList<ProductDocument.PriceItem>)GetVariantPrices(x).ToList());

            IEnumerable<ProductDocument.PriceItem> GetVariantPrices(Variant variant)
            {
                foreach (var price in _priceListItemService.GetByVariant(variant.SystemId).Where(z => z.MinimumQuantity == 0))
                {
                    var priceList = _priceListService.Get<ProductPriceList>(price.PriceListSystemId);
                    if (priceList is null)
                    {
                        continue;
                    }

                    Dictionary<Guid, decimal> appliedCountries;
                    if (priceList.CountryLinks is null || priceList.CountryLinks.Count == 0)
                    {
                        appliedCountries = countries;
                    }
                    else
                    {
                        var priceListCountryLinkSystemIds = priceList.CountryLinks.Select(s => s.CountrySystemId).ToList();
                        appliedCountries = new Dictionary<Guid, decimal>(countries.Where(x => priceListCountryLinkSystemIds.Contains(x.Key)));
                    }

                    foreach (var (countrySystemId, vatRate) in appliedCountries)
                    {
                        var model = new ProductDocument.PriceItem
                        {
                            IsCampaignPrice = false,
                            PriceIncludeVat = priceList.IncludeVat ? price.Price : price.Price * (1 + vatRate),
                            PriceExcludeVat = priceList.IncludeVat ? price.Price / (1 + vatRate) : price.Price,
                            CountrySystemId = countrySystemId
                        };

                        model.PriceListSystemIds.Add(priceList.SystemId);
                        yield return model;
                    }
                }
            }
        }

        private List<ProductDocument> CreateModelsPerChannel(BaseProduct baseProduct, ProductFieldTemplate productFieldTemplate, ref List<Variant> variants)
        {
            List<ProductDocument> productDocuments;
            if (productFieldTemplate.UseVariantUrl)
            {
                variants = variants.OrderBy(x => x.SortIndex).ThenBy(x => x.Id).ToList();
                var groupingField = _templateSettingService.GetTemplateGroupingField(productFieldTemplate.Id);
                productDocuments = new List<ProductDocument>();

                if (string.IsNullOrEmpty(groupingField))
                {
                    // add document for each variant so all of them will appear in search result
                    foreach (var variant in variants)
                    {
                        productDocuments.AddRange(CreateModelPerChannel(new List<Variant> { variant }));
                    }
                }
                else
                {
                    var groupedVariants = variants.GroupBy(x => x.Fields[groupingField]);
                    foreach (var groupedVariantsByField in groupedVariants)
                    {
                        var currentVariants = groupedVariantsByField.ToList();
                        productDocuments.AddRange(CreateModelPerChannel(currentVariants));
                    }
                }
            }
            else
            {
                productDocuments = CreateModelPerChannel(variants);
            }

            return productDocuments;

            List<ProductDocument> CreateModelPerChannel(IList<Variant> variants)
            {
                var mainCategorySystemIds = new Dictionary<Guid, Guid>();
                var categoriesInChannel = new Dictionary<Guid, (HashSet<Guid> Categories, HashSet<Guid> ParentCategories, HashSet<Guid> Assortments, HashSet<Guid> Variants)>();
                foreach (var item in _categoryService.GetByBaseProduct(baseProduct.SystemId)
                    .SelectMany(x => x
                        .ProductLinks
                        .Where(z => z.BaseProductSystemId == baseProduct.SystemId)
                        .Select(z => (Category: x, ProductLink: z)))
                    .Where(x => x.ProductLink.ActiveVariantSystemIds.Intersect(variants.Select(z => z.SystemId)).Any()))
                {
                    var parentCategories = _productIndexingContext.GetParentCatagories(item.Category.SystemId);
                    if (parentCategories.Count > 0)
                    {
                        parentCategories.Reverse();

                        //Only active variants in the category can be indexed
                        var activeVariantsInCurrentCategory = variants.Where(x => item.ProductLink.ActiveVariantSystemIds.Contains(x.SystemId)).ToList();
                        var activeVariantsChannels = activeVariantsInCurrentCategory.SelectMany(x => x.ChannelLinks.Select(y => y.ChannelSystemId)).Distinct().ToList();

                        //The document shouldn't be created if a product doesn't have published variants for a channel.
                        if (activeVariantsChannels.Count == 0)
                        {
                            //Category doesn't have published variants in channels.
                            continue;
                        }

                        var channels = parentCategories.FirstOrDefault()?.ChannelLinks.Select(x => x.ChannelSystemId).ToList() ?? new List<Guid>();

                        //Category and its variants must be published together in the same channels.
                        channels = channels.Intersect(activeVariantsChannels).ToList();

                        if (channels.Count == 0)
                        {
                            //Category is not published together with variants in the same channel.
                            continue;
                        }

                        channels = parentCategories.Aggregate(channels, (current, parent) => current.Intersect(parent.ChannelLinks.Select(x => x.ChannelSystemId)).ToList());

                        if (channels.Count == 0)
                        {
                            //Parent categories are not published together with variants in the same channel.
                            continue;
                        }

                        if (item.ProductLink.MainCategory)
                        {
                            mainCategorySystemIds[item.Category.AssortmentSystemId] = item.Category.SystemId;
                        }

                        foreach (var parent in parentCategories)
                        {
                            foreach (var channelSystemId in channels)
                            {
                                if (!categoriesInChannel.TryGetValue(channelSystemId, out var items))
                                {
                                    items = (new HashSet<Guid>(), new HashSet<Guid>(), new HashSet<Guid>(), new HashSet<Guid>());
                                    categoriesInChannel.Add(channelSystemId, items);
                                }

                                //There can be only one assortment per channel.
                                //If all assortments are indexed there is no need to reindex all products when the assortment is changed on the channel.
                                //The same is for Categories and MainCategories
                                items.Categories.Add(item.Category.SystemId);
                                items.ParentCategories.Add(parent.SystemId);
                                items.Assortments.Add(parent.AssortmentSystemId);
                                //Add only variants which are added to channel
                                items.Variants.UnionWith(activeVariantsInCurrentCategory
                                   .Where(x => x.ChannelLinks.Any(z => z.ChannelSystemId == channelSystemId)
                                       && !string.IsNullOrEmpty(_urlService.GetUrl(x, new ProductUrlArgs(channelSystemId))))
                                   .Select(x => x.SystemId));
                            }
                        }
                    }
                }

                return categoriesInChannel
                    .Where(x => x.Value.Variants.Count > 0)
                    .Select(x => new ProductDocument
                    {
                        ChannelSystemId = x.Key,
                        Assortments = x.Value.Assortments.ToList(),
                        Categories = x.Value.Categories.ToList(),
                        ParentCategories = x.Value.ParentCategories.ToList(),
                        MainCategories = x.Value.Assortments
                            .Select(z => mainCategorySystemIds.TryGetValue(z, out var item) ? new ProductDocument.MainCategory { AssortmentSystemId = z, CategorySystemId = item } : null)
                            .Where(z => !(z is null))
                            .ToList(),
                        VariantSystemIds = x.Value.Variants
                    })
                    .ToList();
            }
        }

        private void PopulateProductDocument(ProductDocument model, CultureInfo cultureInfo, IList<Variant> variants, Context context, bool forBaseProduct)
        {
            if (forBaseProduct)
            {
                model.IsBaseProduct = true;
                model.ArticleNumber = context.BaseProduct.Id.ToLowerInvariant();
                model.Name = context.BaseProduct.Localizations[cultureInfo].Name
                    ?? variants.Select(x => x.Localizations[cultureInfo].Name).FirstOrDefault(x => !string.IsNullOrEmpty(x));

                var links = variants
                    .SelectMany(s => s.ChannelLinks)
                    .Where(x => x.ChannelSystemId == model.ChannelSystemId)
                    .ToList();
                if (links.Count > 0)
                {
                    model.ChannelStartDateTime = links.Select(s => s.StartDateTimeUtc ?? DateTimeOffset.MinValue).Min();
                    model.ChannelEndDateTime = links.Select(s => s.EndDateTimeUtc ?? DateTimeOffset.MaxValue).Max();
                }
            }
            else
            {
                model.ArticleNumber = variants[0].Id.ToLowerInvariant();
                model.Name = variants.Select(x => x.Localizations[cultureInfo].Name).FirstOrDefault(x => !string.IsNullOrEmpty(x))
                    ?? context.BaseProduct.Localizations[cultureInfo].Name;
                var channelLink = variants[0].ChannelLinks.FirstOrDefault(x => x.ChannelSystemId == model.ChannelSystemId);
                if (channelLink is not null)
                {
                    model.ChannelStartDateTime = channelLink.StartDateTimeUtc ?? DateTimeOffset.MinValue;
                    model.ChannelEndDateTime = channelLink.EndDateTimeUtc ?? DateTimeOffset.MaxValue;
                }
            }
            model.BaseProductSystemId = context.BaseProduct.SystemId;
            model.Permissions = context.Permissions;
            model.Organizations = context.BaseProduct.Fields.GetValue<IList<PointerItem>>(ProductFieldNameConstants.OrganizationsPointer)?.Select(x => x.EntitySystemId).ToHashSet()
                                    ?? new HashSet<Guid> { Guid.Empty };

            model.Content = context.GetBaseProductContent(cultureInfo).ToHashSet();
            foreach (var variant in variants)
            {
                model.Content.UnionWith(context.GetVariantContent(cultureInfo, variant));
            }

            PopulatePrices(model, context, variants);
            PopulatePurchaseHistoryData(model, context, variants);

            PopulatePlanning(model, context.BaseProduct, variants, forBaseProduct);
            PopulateNewsDate(model, cultureInfo, context.BaseProduct, variants, forBaseProduct);

            var fieldContainer = (forBaseProduct
                ? new[] { context.BaseProduct.Fields }.Concat(variants.Select(x => x.Fields))
                : variants.Select(x => x.Fields).Concat(new[] { context.BaseProduct.Fields })
            ).ToList();
            PopulateTags(model, cultureInfo, fieldContainer);
        }

        private void PopulatePurchaseHistoryData(ProductDocument model, Context context, IList<Variant> variants)
        {
            foreach (var variant in variants)
            {
                if (context.PurchaseHistory.TryGetValue(variant.SystemId, out var items)
                    && items.TryGetValue(model.ChannelSystemId, out var value))
                {
                    model.PurchaseHistoryQuantity += value;
                }
            }
        }

        private void PopulateNewsDate(ProductDocument model, CultureInfo cultureInfo, BaseProduct baseProduct, IList<Variant> variants, bool forBaseProduct)
        {
            const string field = "News";
            model.NewsDate = forBaseProduct
                ? (baseProduct.Fields.GetValue<DateTimeOffset?>(field, cultureInfo.Name) ?? variants.Select(x => x.Fields.GetValue<DateTimeOffset?>(field, cultureInfo.Name)).Where(x => x.HasValue).FirstOrDefault())?.UtcDateTime
                : (variants.Select(x => x.Fields.GetValue<DateTimeOffset?>(field, cultureInfo.Name)).Where(x => x.HasValue).FirstOrDefault() ?? baseProduct.Fields.GetValue<DateTimeOffset?>(field, cultureInfo.Name))?.UtcDateTime;
        }

        private void PopulatePlanning(ProductDocument model, BaseProduct baseProduct, IList<Variant> variants, bool forBaseProduct)
        {
            foreach (var item in _productListItemService.GetByBaseProduct(baseProduct.SystemId)
                .Where(x => x.ActiveVariantSystemIds.Intersect(variants.Select(z => z.SystemId)).Any()))
            {
                model.ProductLists.Add(item.ProductListSystemId);

                var productList = _productListService.Get<StaticProductList>(item.ProductListSystemId);
                var index = (forBaseProduct
                    ? new[] { productList?.ProductsSortOrder.IndexOf(baseProduct.SystemId) ?? -1 }
                    : variants.Select(variant => productList?.ProductsSortOrder.IndexOf(variant.SystemId) ?? -1))
                    .Select(x => x == -1 ? int.MaxValue : x).Min();
                model.ProductListSortIndex.Add(new ProductDocument.SortItem { SystemId = item.ProductListSystemId, SortIndex = index });
            }

            foreach (var item in _categoryService.GetByBaseProduct(baseProduct.SystemId)
                .SelectMany(x => x
                    .ProductLinks
                    .Where(z => z.BaseProductSystemId == baseProduct.SystemId)
                    .Select(z => (Category: x, Link: z)))
                .Where(x => x.Link.ActiveVariantSystemIds.Intersect(variants.Select(z => z.SystemId)).Any()))
            {
                var index = (forBaseProduct
                    ? new[] { item.Category.ProductsSortOrder.IndexOf(baseProduct.SystemId) }
                    : variants.Select(variant => item.Category.ProductsSortOrder.IndexOf(variant.SystemId)))
                    .Select(x => x == -1 ? int.MaxValue : x).Min();
                model.CategorySortIndex.Add(new ProductDocument.SortItem { SystemId = item.Category.SystemId, SortIndex = index });
            }
        }

        private void PopulatePrices(ProductDocument model, Context context, IList<Variant> variants)
        {
            if (!context.ChannelCountryLinks.TryGetValue(model.ChannelSystemId, out var countrySystemIds) || countrySystemIds.Count == 0)
            {
                return;
            }

            var allPrices = variants.Select(x => (context.Prices.TryGetValue(x.SystemId, out var prices), prices))
                .Where(x => x.Item1)
                .SelectMany(x => x.prices)
                .Where(x => countrySystemIds.Contains(x.CountrySystemId))
                .ToList();

            foreach (var price in allPrices
                .Where(x => !x.IsCampaignPrice))
            {
                var priceModel = model.Prices.FirstOrDefault(x =>
                    x.PriceIncludeVat == price.PriceIncludeVat
                    && x.CountrySystemId == price.CountrySystemId
                    && !x.IsCampaignPrice);

                if (priceModel is null)
                {
                    model.Prices.Add(priceModel = new ProductDocument.PriceItem
                    {
                        CountrySystemId = price.CountrySystemId,
                        PriceIncludeVat = price.PriceIncludeVat,
                        PriceExcludeVat = price.PriceExcludeVat
                    });
                }

                priceModel.PriceListSystemIds.UnionWith(price.PriceListSystemIds);
            }

            foreach (var item in allPrices
                .Where(x => x.IsCampaignPrice)
                .GroupBy(x => new { x.PriceListSystemIds, x.CountrySystemId })
                .Select(x => new { x.Key, PriceIncludeVat = x.Min(z => z.PriceIncludeVat), PriceExcludeVat = x.Min(z => z.PriceExcludeVat) }))
            {
                var priceModel = model.Prices.FirstOrDefault(x =>
                    x.PriceIncludeVat == item.PriceIncludeVat
                    && x.CountrySystemId == item.Key.CountrySystemId
                    && x.IsCampaignPrice);

                if (priceModel is null)
                {
                    model.Prices.Add(priceModel = new ProductDocument.PriceItem
                    {
                        IsCampaignPrice = true,
                        CountrySystemId = item.Key.CountrySystemId,
                        PriceIncludeVat = item.PriceIncludeVat,
                        PriceExcludeVat = item.PriceExcludeVat
                    });
                }

                priceModel.PriceListSystemIds.UnionWith(item.Key.PriceListSystemIds);
            }
        }

        private void PopulateTags(ProductDocument model, CultureInfo cultureInfo, IEnumerable<MultiCultureFieldContainer> fieldContainers)
        {
            foreach (var filterField in _filterService.GetProductFilteringFields())
            {
                switch (filterField)
                {
                    case FilteringConstants.FilterNews:
                    case FilteringConstants.FilterPrice:
                    case FilteringConstants.FilterProductCategories:
                        continue;
                }

                var field = _fieldDefinitionService.Get<ProductArea>(filterField);
                if (field is null)
                {
                    continue;
                }

                var values = fieldContainers
                    .Select(x => field.MultiCulture ? x.GetValue<object>(filterField, cultureInfo) : x.GetValue<object>(filterField))
                    .Where(x => !(x is null)).Distinct().ToList();

                if (values.Count == 0)
                {
                    continue;
                }

                switch (field.FieldType)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    case SystemFieldTypeConstants.CustomerPointerOrganization:
#pragma warning restore CS0618 // Type or member is obsolete
                    case SystemFieldTypeConstants.Date:
                    case SystemFieldTypeConstants.DateTime:
                    case SystemFieldTypeConstants.Editor:
                    case SystemFieldTypeConstants.MediaPointerFile:
                    case SystemFieldTypeConstants.MediaPointerImage:
                    case SystemFieldTypeConstants.MultiField:
                    case SystemFieldTypeConstants.MultirowText:
                    case SystemFieldTypeConstants.Object:
                    case SystemFieldTypeConstants.Pointer:
                    case SystemFieldTypeConstants.Link:
                        // skip adding the tag, it is data that not should be filtered on
                        continue;

                    case SystemFieldTypeConstants.Boolean:
                        foreach (var value in values)
                        {
                            model.Tags.Add(new ProductDocument.TagItem { Key = filterField, Value = ((bool)value).ToString(cultureInfo) });
                        }
                        break;

                    case SystemFieldTypeConstants.Decimal:
                        foreach (var value in values)
                        {
                            model.Tags.Add(new ProductDocument.TagItem { Key = filterField, Value = ((decimal)value).ToString("0.########", cultureInfo) });
                        }
                        break;

                    case SystemFieldTypeConstants.DecimalOption:
                        foreach (var value in values)
                        {
                            if (value is IList<decimal> decimalValues)
                            {
                                foreach (var item in OptionValueHelper.GetDecimalOptionValues((DecimalOption)field.Option, decimalValues, cultureInfo))
                                {
                                    AddOptionTag(model, filterField, item);
                                }
                            }
                            else
                            {
                                AddOptionTag(model, filterField, OptionValueHelper.GetDecimalOptionValue((DecimalOption)field.Option, (decimal)value, cultureInfo));
                            }
                        }
                        break;

                    case SystemFieldTypeConstants.Int:
                        foreach (var value in values)
                        {
                            model.Tags.Add(new ProductDocument.TagItem { Key = filterField, Value = ((int)value).ToString(cultureInfo) });
                        }
                        break;

                    case SystemFieldTypeConstants.IntOption:
                        foreach (var value in values)
                        {
                            if (value is IList<int> intValues)
                            {
                                foreach (var item in OptionValueHelper.GetIntOptionValues((IntOption)field.Option, intValues, cultureInfo))
                                {
                                    AddOptionTag(model, filterField, item);
                                }
                            }
                            else
                            {
                                AddOptionTag(model, filterField, OptionValueHelper.GetIntOptionValue((IntOption)field.Option, (int)value, cultureInfo));
                            }
                        }
                        break;

                    case SystemFieldTypeConstants.Long:
                        foreach (var value in values)
                        {
                            model.Tags.Add(new ProductDocument.TagItem { Key = filterField, Value = ((long)value).ToString(cultureInfo) });
                        }
                        break;

                    case SystemFieldTypeConstants.TextOption:
                        foreach (var value in values)
                        {
                            if (value is IList<string> textValues)
                            {
                                foreach (var item in OptionValueHelper.GetTextOptionValues((TextOption)field.Option, textValues, cultureInfo))
                                {
                                    AddOptionTag(model, filterField, item);
                                }
                            }
                            else
                            {
                                AddOptionTag(model, filterField, OptionValueHelper.GetTextOptionValue((TextOption)field.Option, (string)value, cultureInfo));
                            }
                        }
                        break;

                    case SystemFieldTypeConstants.LimitedText:
                    case SystemFieldTypeConstants.Text:
                    default:
                        foreach (var value in values)
                        {
                            model.Tags.Add(new ProductDocument.TagItem { Key = filterField, Value = value.ToString() });
                        }

                        break;
                }
            }

            void AddOptionTag(ProductDocument model, string filterField, string item)
            {
                if (!model.Tags.Any(x => x.Key == filterField && x.Value == item))
                {
                    model.Tags.Add(new ProductDocument.TagItem { Key = filterField, Value = item });
                }
            }
        }

        private class Context
        {
            private readonly ContentBuilderService _contentBuilderService;
            private readonly Dictionary<Guid, Dictionary<CultureInfo, ISet<string>>> _contentCache
                = new Dictionary<Guid, Dictionary<CultureInfo, ISet<string>>>();

            public Context(ContentBuilderService contentBuilderService)
            {
                _contentBuilderService = contentBuilderService;
            }

            public List<Variant> UsedVariants { get; set; }
            public IReadOnlyCollection<string> Permissions { get; set; }
            public IDictionary<Guid, IList<ProductDocument.PriceItem>> Prices { get; set; }
            public IDictionary<Guid, IDictionary<Guid, decimal>> PurchaseHistory { get; set; }

            public IDictionary<Guid, HashSet<Guid>> ChannelCountryLinks { get; set; }

            public BaseProduct BaseProduct { get; set; }
            public ProductFieldTemplate FieldTemplate { get; set; }

            public ISet<string> GetBaseProductContent(CultureInfo cultureInfo)
            {
                if (!_contentCache.TryGetValue(BaseProduct.SystemId, out var cultureCache))
                {
                    _contentCache.Add(BaseProduct.SystemId, cultureCache = new Dictionary<CultureInfo, ISet<string>>());
                }

                if (!cultureCache.TryGetValue(cultureInfo, out var items))
                {
                    cultureCache.Add(cultureInfo,
                        items = _contentBuilderService.BuildContent<ProductFieldTemplate, ProductArea>(FieldTemplate.SystemId, cultureInfo, BaseProduct.Fields));
                }

                return items;
            }

            public ISet<string> GetVariantContent(CultureInfo cultureInfo, Variant variant)
            {
                if (!_contentCache.TryGetValue(variant.SystemId, out var cultureCache))
                {
                    _contentCache.Add(variant.SystemId, cultureCache = new Dictionary<CultureInfo, ISet<string>>());
                }

                if (!cultureCache.TryGetValue(cultureInfo, out var items))
                {
                    cultureCache.Add(cultureInfo,
                        items = _contentBuilderService.BuildContent<ProductFieldTemplate, ProductArea>(FieldTemplate.SystemId, cultureInfo, variant.Fields));
                }

                return items;
            }
        }
    }
}
