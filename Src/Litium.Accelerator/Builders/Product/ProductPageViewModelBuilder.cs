using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Caching;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Services;
using Litium.Accelerator.ViewModels.Brand;
using Litium.Accelerator.ViewModels.Product;
using Litium.FieldFramework;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Products;
using Litium.Web.Models.Websites;

namespace Litium.Accelerator.Builders.Product
{
    public class ProductPageViewModelBuilder : IViewModelBuilder<ProductPageViewModel>
    {
        private readonly ProductItemViewModelBuilder _itemViewModelBuilder;
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly ProductService _productService;
        private readonly PageByFieldTemplateCache<BrandPageFieldTemplateCache> _brandPageByFieldTypeCache;
        private readonly ProductModelBuilder _productModelBuilder;
        private readonly FieldDefinitionService _fieldDefinitionService;
        private readonly ProductFieldGroupViewModelBuilder _productFieldGroupViewModelBuilder;

        public ProductPageViewModelBuilder(
            ProductItemViewModelBuilder itemViewModelBuilder,
            RequestModelAccessor requestModelAccessor,
            ProductService productService,
            PageByFieldTemplateCache<BrandPageFieldTemplateCache> brandPageByFieldTypeCache,
            ProductModelBuilder productModelBuilder,
            FieldDefinitionService fieldDefinitionService,
            ProductFieldGroupViewModelBuilder productFieldGroupViewModelBuilder)
        {
            _itemViewModelBuilder = itemViewModelBuilder;
            _requestModelAccessor = requestModelAccessor;
            _productService = productService;
            _brandPageByFieldTypeCache = brandPageByFieldTypeCache;
            _productModelBuilder = productModelBuilder;
            _fieldDefinitionService = fieldDefinitionService;
            _productFieldGroupViewModelBuilder = productFieldGroupViewModelBuilder;
        }

        public async Task<ProductPageViewModel> BuildAsync(BaseProduct baseProduct)
        {
            var productModel = _productModelBuilder.BuildFromBaseProduct(baseProduct);
            var viewModel = Build(productModel);

            viewModel.Variants = GetVariants(productModel, _requestModelAccessor.RequestModel.WebsiteModel.SystemId, _requestModelAccessor.RequestModel.ChannelModel.SystemId);
            viewModel.MostSoldProducts = await GetMostSoldProductsAsync(baseProduct.Id);

            return viewModel;
        }

        public async Task<ProductPageViewModel> BuildAsync(Variant variant)
        {
            var productModel = _productModelBuilder.BuildFromVariant(variant);
            var viewModel = Build(productModel);

            viewModel.BundleProducts = GetBundles(variant);
            viewModel.MostSoldProducts = await GetMostSoldProductsAsync(variant.Id);
            SetFilters(viewModel, productModel, _requestModelAccessor.RequestModel.WebsiteModel.SystemId, _requestModelAccessor.RequestModel.ChannelModel.SystemId);

            return viewModel;
        }

        private ProductPageViewModel Build(ProductModel productModel)
        {
            var viewModel = productModel.MapTo<ProductPageViewModel>();

            viewModel.ProductItem = _itemViewModelBuilder.Build(productModel, false);

            viewModel.ProductFieldGroups = _productFieldGroupViewModelBuilder.Build(productModel);

            viewModel.BrandPage = GetBrandPage(viewModel.ProductItem.Brand);
            viewModel.AccessoriesProducts = GetRelatedProducts(productModel, RelationTypeNameConstants.Accessory);
            viewModel.SimilarProducts = GetRelatedProducts(productModel, RelationTypeNameConstants.SimilarProducts);

            return viewModel;
        }

        private KeyValuePair<RelationshipType, List<ProductItemViewModel>> GetRelatedProducts(ProductModel productModel, string typeName)
        {
            var relatedModel = _productService.GetProductRelationships(productModel, typeName);
            if (relatedModel == null)
            {
                return default;
            }

            var relatedProducts = relatedModel.Items
                .Where(x => x.Product != null)
                .Select(x => _itemViewModelBuilder.Build(x.Product.SelectedVariant))
                .Where(x => x != null)
                .ToList();

            return new KeyValuePair<RelationshipType, List<ProductItemViewModel>>(relatedModel.RelationshipType, relatedProducts);
        }

        private BrandViewModel GetBrandPage(string brand)
        {
            if (string.IsNullOrEmpty(brand))
            {
                return null;
            }
            BrandViewModel result = null;
            _brandPageByFieldTypeCache.TryFindPage(page =>
            {
                var pageName = page.Fields.GetValue<string>(SystemFieldDefinitionConstants.Name, CultureInfo.CurrentUICulture.Name);
                if (pageName != null && pageName.Equals(brand, StringComparison.Ordinal))
                {
                    result = page.MapTo<PageModel>().MapTo<BrandViewModel>();
                    return true;
                }

                return false;
            });

            return result;
        }

        private List<ProductItemViewModel> GetBundles(Variant variant)
        {
            return variant.BundledVariants.Select(item => _itemViewModelBuilder.Build(item.BundledVariantSystemId.MapTo<Variant>())).Where(x => x != null).ToList();
        }

        private async Task<List<ProductItemViewModel>> GetMostSoldProductsAsync(string articleNumber)
        {
            return (await _productService
                .GetMostSoldProductsAsync(
                    _requestModelAccessor.RequestModel.ChannelModel.SystemId,
                    articleNumber: articleNumber))
                .Select(x => _itemViewModelBuilder.Build(x))
                .Where(x => x != null)
                .ToList();
        }

        private List<ProductItemViewModel> GetVariants(ProductModel productModel, Guid websiteId, Guid channelId)
        {
            return productModel.BaseProduct
                .GetPublishedVariants(websiteId, channelId)
                .Select(variant => _itemViewModelBuilder.Build(variant))
                .Where(x => x != null)
                .ToList();
        }

        private void SetFilters(ProductPageViewModel model, ProductModel productModel, Guid webSiteId, Guid channelId)
            => SetFilters(model, productModel.BaseProduct, productModel.SelectedVariant, webSiteId, channelId);

        private void SetFilters(ProductPageViewModel model, BaseProduct baseProduct, Variant currentVariant, Guid webSiteId, Guid channelId)
        {
            if (currentVariant == null)
            {
                return;
            }

            var cultureInfo = CultureInfo.CurrentUICulture;
            var colorDefinition = _fieldDefinitionService.Get<ProductArea>("Color");
            var sizeDefinition = _fieldDefinitionService.Get<ProductArea>("Size");
            model.ColorText = colorDefinition?.Localizations[cultureInfo].Name ?? "Color";
            model.SizeText = sizeDefinition?.Localizations[cultureInfo].Name ?? "Size";

            var currentColorKey = currentVariant.Fields["Color"] as string;
            var currentSizeKey = currentVariant.Fields["Size"] as string;
            var colorFilterItems = new List<ProductPageViewModel.ProductFilterItem>();
            var sizeFilterItems = new List<ProductPageViewModel.ProductFilterItem>();

            var allColors = new List<Tuple<string, string>>();
            var allSizes = new List<Tuple<string, string>>();

            var sizesForColor = new List<string>();
            var colorsForSize = new List<string>();
            foreach (var variant in baseProduct.GetPublishedVariants(webSiteId, channelId))
            {
                // Get the "Color" and Size
                var variantColorKey = variant.Fields["Color"] as string;
                var variantSizeKey = variant.Fields["Size"] as string;
                var variantColor = colorDefinition.GetTranslation(variantColorKey, cultureInfo);
                var variantSize = sizeDefinition.GetTranslation(variantSizeKey, cultureInfo);

                if (!string.IsNullOrEmpty(variantColor))
                {
                    if (allColors.All(x => x.Item1 != variantColorKey))
                    {
                        allColors.Add(Tuple.Create(variantColorKey, variantColor));
                    }

                    colorFilterItems.Add(new ProductPageViewModel.ProductFilterItem(variant, currentColorKey, variantSizeKey, variantColorKey, variantColor, webSiteId, channelId));

                    //Add available sizes for the selected color
                    if (variantColorKey == currentColorKey
                        && !sizesForColor.Contains(variantSizeKey))
                    {
                        sizesForColor.Add(variantSizeKey);
                    }
                }

                if (!string.IsNullOrEmpty(variantSize))
                {
                    if (allSizes.All(x => x.Item1 != variantSizeKey))
                    {
                        allSizes.Add(Tuple.Create(variantSizeKey, variantSize));
                    }

                    sizeFilterItems.Add(new ProductPageViewModel.ProductFilterItem(variant, currentSizeKey, variantColorKey, variantSizeKey, variantSize, webSiteId, channelId));

                    //Add available colors for the selected size
                    if (variantSizeKey == currentSizeKey
                        && !colorsForSize.Contains(variantColorKey))
                    {
                        colorsForSize.Add(variantColorKey);
                    }
                }
            }

            //Filter colors
            foreach (var color in allColors)
            {
                //Leave color with the selected size
                var colorItem = colorFilterItems.FirstOrDefault(item => item.Value == color.Item1 & item.SelectedFilter == currentSizeKey);
                if (colorItem != null)
                {
                    colorFilterItems.RemoveAll(item => item.Value == color.Item1 & item.SelectedFilter != currentSizeKey);
                }

                var colors = colorFilterItems.Where(item => item.Value == color.Item1).ToList();
                foreach (var item in colors)
                {
                    item.Enabled = colorsForSize.Contains(color.Item1);
                }
            }

            foreach (var size in allSizes)
            {
                //Leave size with the selected color
                var sizeItem = sizeFilterItems.FirstOrDefault(item => item.Value == size.Item1 & item.SelectedFilter == currentColorKey);
                if (sizeItem != null)
                {
                    sizeFilterItems.RemoveAll(item => item.Value == size.Item1 & item.SelectedFilter != currentColorKey);
                }
                var sizes = sizeFilterItems.Where(item => item.Value == size.Item1).ToList();
                foreach (var item in sizes)
                {
                    item.Enabled = sizesForColor.Contains(size.Item1);
                }
            }

            //Clear filters from empty values(if article doesn't have filter)
            colorFilterItems.RemoveAll(item => item.Value == string.Empty);
            sizeFilterItems.RemoveAll(item => item.Value == string.Empty);

            if (colorFilterItems.Count > 0)
            {
                model.Colors = colorFilterItems.OrderBy(xx => xx.Title).GroupBy(x => x.Value).Select(y => y.First()).ToList();
            }
            if (sizeFilterItems.Count > 0)
            {
                model.Sizes = sizeFilterItems.OrderBy(xx => xx.Title).GroupBy(x => x.Value).Select(y => y.First()).ToList();
            }
        }
    }
}

