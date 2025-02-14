using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Search;
using Litium.Accelerator.Search.Filtering;
using Litium.Accelerator.ViewModels.Brand;
using Litium.Accelerator.ViewModels.Search;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Products.Routing;
using Litium.Web.Routing;

namespace Litium.Accelerator.Builders.Search
{
    [Service(ServiceType = typeof(FilterViewModelBuilder), Lifetime = DependencyLifetime.Scoped)]
    public class FilterViewModelBuilder : IViewModelBuilder<FilterResult>
    {
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly RouteRequestInfoAccessor _routeRequestInfoAccessor;
        private readonly CategoryFilterService _categoryFilterService;
        private readonly FilterAggregator _filterAggregator;

        public FilterViewModelBuilder(
            RequestModelAccessor requestModelAccessor,
            CategoryFilterService categoryFilterService,
            RouteRequestInfoAccessor routeRequestInfoAccessor,
            FilterAggregator filterAggregator)
        {
            _requestModelAccessor = requestModelAccessor;
            _categoryFilterService = categoryFilterService;
            _routeRequestInfoAccessor = routeRequestInfoAccessor;
            _filterAggregator = filterAggregator;
        }

        public async Task<FilterResult> BuildAsync()
        {
            var productCatalogData = _routeRequestInfoAccessor.RouteRequestInfo?.Data as ProductPageData;
            var pageModel = _requestModelAccessor.RequestModel.CurrentPageModel;
            if (productCatalogData == null && !pageModel.IsBrandPageType() && !pageModel.IsProductListPageType() && !pageModel.IsSearchResultPageType())
            {
                return null;
            }

            var filterNavigation = _requestModelAccessor.RequestModel.WebsiteModel.GetNavigationType() == NavigationType.Filter;
            var searchQuery = _requestModelAccessor.RequestModel.SearchQuery;
            if ((searchQuery.Type != SearchType.Products && !filterNavigation) || (pageModel.IsSearchResultPageType() && string.IsNullOrEmpty(searchQuery.Text)))
            {
                return null;
            }

            var propertyNames = GetPropertyNames(searchQuery);

            return new FilterResult
            {
                Items = (await _filterAggregator.GetFilterAsync(searchQuery, propertyNames)).ToList()
            };
        }

        private IEnumerable<string> GetPropertyNames(SearchQuery searchQuery)
        {
            var filters = _requestModelAccessor.RequestModel.WebsiteModel.GetValue<IList<string>>(AcceleratorWebsiteFieldNameConstants.FiltersOrdering);
            if (filters == null)
            {
                return new string[0];
            }

            if (searchQuery.CategorySystemId != null)
            {
                var (result, isOrdered) = _categoryFilterService.GetFilters(searchQuery.CategorySystemId.Value);
                if (result != null)
                {
                    var categoryFilters = result
                        .Select(x => new Tuple<string, int>(x, filters.IndexOf(x)))
                        .Where(x => x.Item2 != -1);

                    if (!isOrdered)
                    {
                        categoryFilters = categoryFilters.OrderBy(x => x.Item2);
                    }

                    return categoryFilters.Select(x => x.Item1).ToList();
                }
            }

            var page = _requestModelAccessor.RequestModel.CurrentPageModel;

            //Current page is the search result pagetype
            if (page.IsSearchResultPageType())
            {
                return new[] { FilteringConstants.FilterProductCategories }.Concat(filters);
            }

            //Current page is the brand pagetype
            if (page.IsBrandPageType())
            {
                return filters.Except(new[] { BrandListViewModel.TagName }, StringComparer.OrdinalIgnoreCase);
            }

            return filters;
        }
    }
}
