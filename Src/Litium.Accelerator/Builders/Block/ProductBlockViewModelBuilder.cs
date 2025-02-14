using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Builders.Product;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Search;
using Litium.Accelerator.ViewModels.Block;
using Litium.Accelerator.ViewModels.Search;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Blocks;
using Litium.Web.Models.Products;

namespace Litium.Accelerator.Builders.Block
{
    public class ProductBlockViewModelBuilder : IViewModelBuilder<ProductBlockViewModel>
    {
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly ProductModelBuilder _productModelBuilder;
        private readonly VariantService _variantService;
        private readonly BaseProductService _baseProductService;
        private readonly ProductSearchService _productSearchService;
        private readonly ProductItemViewModelBuilder _productItemViewModelBuilder;

        public ProductBlockViewModelBuilder(ProductModelBuilder productModelBuilder,
            ProductItemViewModelBuilder productItemViewModelBuilder,
            RequestModelAccessor requestModelAccessor,
            ProductSearchService productSearchService,
            BaseProductService baseProductService,
            VariantService variantService)
        {
            _productModelBuilder = productModelBuilder;
            _productItemViewModelBuilder = productItemViewModelBuilder;
            _requestModelAccessor = requestModelAccessor;
            _productSearchService = productSearchService;
            _baseProductService = baseProductService;
            _variantService = variantService;
        }

        /// <summary>
        /// Build the product block view model
        /// </summary>
        /// <param name="blockModel">The current product block</param>
        /// <returns>Return the product block view model</returns>
        public virtual async Task<ProductBlockViewModel> BuildAsync(BlockModel blockModel)
        {
            var data = blockModel.MapTo<ProductBlockViewModelData>();
            var viewModel = new ProductBlockViewModel();
            var channel = _requestModelAccessor.RequestModel.ChannelModel.Channel;

            if (data != null)
            {
                viewModel.FooterLinkText = data.LinkText;
                if (data.LinkToCategorySystemId != Guid.Empty)
                {
                    viewModel.FooterLinkUrl = data.LinkToCategorySystemId.MapTo<Category>().GetUrl(_requestModelAccessor.RequestModel.ChannelModel.SystemId, true);
                }
                else if (!string.IsNullOrEmpty(data.LinkToPage.Href))
                {
                    viewModel.FooterLinkUrl = data.LinkToPage.Href;
                }

                viewModel.Title = data.Title;

                var products = new List<ProductModel>();
                if (data.SectionProductsType == BlockProductsType.Products)
                {
                    products.AddRange(data.ProductSystemIds.Select(x => _productModelBuilder.BuildFromVariant(_variantService.Get(x)) ?? _productModelBuilder.BuildFromBaseProduct(_baseProductService.Get(x), channel)).Where(x => x != null));
                }
                else
                {
                    var searchQuery = new SearchQuery
                    {
                        PageSize = data.NumberOfProducts
                    };

                    switch (data.SectionProductsType)
                    {
                        case BlockProductsType.Category:
                            searchQuery.CategoryShowRecursively = true;
                            searchQuery.SortBy = data.ProductSorting;
                            searchQuery.CategorySystemId = data.CategorySystemId;
                            break;
                        case BlockProductsType.ProductList:
                            searchQuery.ProductListSystemId = data.ProductListSystemId;
                            break;
                    }
                    var items = (await _productSearchService.SearchAsync(searchQuery))?.Items.Value;
                    if (items != null)
                    {
                        products.AddRange(items.OfType<ProductSearchResult>().Select(x => x.Item));
                    }
                }

                viewModel.Products = products.Select(x => _productItemViewModelBuilder.Build(x)).ToList();
            }
            return viewModel;
        }
    }
}
