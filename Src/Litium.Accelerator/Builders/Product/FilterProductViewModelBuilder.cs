using System.Threading.Tasks;
using Litium.Accelerator.Builders.Brand;
using Litium.Accelerator.Builders.Search;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Routing;
using Litium.Accelerator.ViewModels.Product;
using Litium.Web.Products.Routing;
using Litium.Web.Routing;

namespace Litium.Accelerator.Builders.Product
{
    public class FilterProductViewModelBuilder : IViewModelBuilder<FilterProductViewModel>
    {
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly BrandViewModelBuilder _brandViewModelBuilder;
        private readonly CategoryPageViewModelBuilder _categoryPageViewModelBuilder;
        private readonly RouteRequestInfoAccessor _routeRequestInfoAccessor;
        private readonly SearchResultViewModelBuilder _searchResultViewModelBuilder;
        private readonly ProductListViewModelBuilder _productListViewModelBuilder;

        public FilterProductViewModelBuilder(
            RequestModelAccessor requestModelAccessor,
            BrandViewModelBuilder brandViewModelBuilder,
            CategoryPageViewModelBuilder categoryPageViewModelBuilder,
            RouteRequestInfoAccessor routeRequestInfoAccessor,
            SearchResultViewModelBuilder searchResultViewModelBuilder,
            ProductListViewModelBuilder productListViewModelBuilder)
        {
            _requestModelAccessor = requestModelAccessor;
            _brandViewModelBuilder = brandViewModelBuilder;
            _categoryPageViewModelBuilder = categoryPageViewModelBuilder;
            _routeRequestInfoAccessor = routeRequestInfoAccessor;
            _searchResultViewModelBuilder = searchResultViewModelBuilder;
            _productListViewModelBuilder = productListViewModelBuilder;
        }

        public virtual async Task<FilterProductViewModel> BuildAsync()
        {
            var page = _requestModelAccessor.RequestModel.CurrentPageModel;
            IViewModel viewData = null;
            int totalCount = 0;

            if (page.IsBrandPageType())
            {
                //Brand Page Type
                var model = await _brandViewModelBuilder.BuildAsync(page);
                totalCount = model?.Pagination?.TotalCount ?? 0;
                viewData = model;
            }
            else if (page.IsSearchResultPageType())
            {
                //Search Result Page Type
                var model = await _searchResultViewModelBuilder.BuildAsync();
                totalCount = model?.Pagination?.TotalCount ?? 0;
                viewData = model;
            }
            else if (page.IsProductListPageType())
            {
                //Product List Page Type
                var model = await _productListViewModelBuilder.BuildAsync();
                totalCount = model?.Pagination?.TotalCount ?? 0;
                viewData = model;
            }
            else
            {
                if (_routeRequestInfoAccessor.RouteRequestInfo?.Data is ProductPageData productData)
                {
                    var model = await _categoryPageViewModelBuilder.BuildAsync(productData.CategorySystemId.Value);
                    totalCount = model?.Pagination?.TotalCount ?? 0;
                    viewData = model;
                }
            }

            if (viewData != null)
            {
                return new FilterProductViewModel()
                {
                    ViewData = viewData,
                    TotalCount = totalCount
                };
            }

            return null;
        }
    }
}
