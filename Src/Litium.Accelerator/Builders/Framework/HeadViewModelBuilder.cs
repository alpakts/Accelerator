using System;
using System.Linq;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Search;
using Litium.Accelerator.Search.Filtering;
using Litium.Accelerator.ViewModels.Framework;
using Litium.Web;
using Litium.Web.Models.Websites;
using Litium.Web.Products.Routing;
using Litium.Web.Routing;

namespace Litium.Accelerator.Builders.Framework
{
    public class HeadViewModelBuilder<TViewModel> : IViewModelBuilder<TViewModel>
        where TViewModel : HeadViewModel
    {
        private readonly MetaService _metaService;
        private readonly OpenGraphViewModelBuilder _openGraphViewModelBuilder;
        private readonly TrackingScriptService _trackingScriptService;
        private readonly FaviconViewModelBuilder _faviconViewModelBuilder;
        private readonly FilterService _filterService;
        private readonly MetaService.CanonicalSettings _canonicalSettings;
        private readonly MetaService.RobotsSettings _robotsSettings;
        private readonly RouteRequestInfoAccessor _routeRequestInfoAccessor;
        private readonly RequestModelAccessor _requestModelAccessor;

        public HeadViewModelBuilder(RouteRequestInfoAccessor routeRequestInfoAccessor, RequestModelAccessor requestModelAccessor, MetaService metaService,OpenGraphViewModelBuilder openGraphViewModelBuilder,MetaService.CanonicalSettings canonicalSettings,MetaService.RobotsSettings robotsSettings,TrackingScriptService trackingScriptService,FaviconViewModelBuilder faviconViewModelBuilder, FilterService filterService)
        {
            _routeRequestInfoAccessor = routeRequestInfoAccessor;
            _requestModelAccessor = requestModelAccessor;
            _metaService = metaService;
            _openGraphViewModelBuilder = openGraphViewModelBuilder;
            _trackingScriptService = trackingScriptService;
            _faviconViewModelBuilder = faviconViewModelBuilder;
            _canonicalSettings = canonicalSettings;
            _robotsSettings = robotsSettings;
            _filterService = filterService;
        }

        public HeadViewModel Build()
        {
            var websiteModel = _requestModelAccessor.RequestModel.WebsiteModel;
            var pageModel = _requestModelAccessor.RequestModel.CurrentPageModel;
            var categoryModel = _requestModelAccessor.RequestModel.CurrentCategoryModel;
            var productModel = _requestModelAccessor.RequestModel.CurrentProductModel;

            SetMetaInfo(pageModel);
            var viewModel = new HeadViewModel();
            var favicon = websiteModel.Website.Fields.GetValue<Guid?>(AcceleratorWebsiteFieldNameConstants.LogotypeIcon);
            if (favicon.HasValue)
            {
                viewModel.Favicons = _faviconViewModelBuilder.Build(favicon.Value);
            }
            var variant = productModel?.UseVariantUrl == true ? productModel?.SelectedVariant : default;
            viewModel.MetaDescription = _metaService.GetDescription(pageModel.Page, categoryModel?.Category, productModel?.BaseProduct, variant);
            viewModel.Canonical = _metaService.GetCanonical(pageModel.Page, categoryModel?.Category, productModel?.BaseProduct, variant);
            viewModel.MetaRobots = _metaService.GetRobots(pageModel.Page, categoryModel?.Category, productModel?.BaseProduct);
            viewModel.Title = $"{_metaService.GetTitle(pageModel.Page, categoryModel?.Category, productModel?.BaseProduct, variant)} {"site.title".AsWebsiteText()}";
            viewModel.OpenGraph = _openGraphViewModelBuilder.Build(websiteModel.Website, pageModel.Page, categoryModel?.Category, productModel?.BaseProduct, variant);
            viewModel.TrackingScripts = _trackingScriptService.GetHeaderScripts(pageModel.Page);
            return viewModel;
        }

        private void SetMetaInfo(PageModel currentPage)
        {
            var searchQuery = _requestModelAccessor.RequestModel.SearchQuery.Clone(); ;
            if (!currentPage.IsSearchResultPageType() && !currentPage.IsBrandPageType() &&
                !currentPage.IsProductListPageType() &&
                (!(_routeRequestInfoAccessor.RouteRequestInfo.Data is ProductPageData productData) ||
                 productData.BaseProductSystemId != null))
            {
                return;
            }
            
            var firstFilterName = searchQuery.Tags.Keys.Select(x => x.ToLowerInvariant()).FirstOrDefault();
            var indexFirstFilterName = firstFilterName != null && _filterService.IndexFilter(firstFilterName);

            if (searchQuery.ContainsMultipleFilters())
            {
                _robotsSettings.Index = false;
            }
            else if (searchQuery.ContainsFilter())
            {
                if (!indexFirstFilterName)
                {
                    _robotsSettings.Index = false;
                }
                else if (searchQuery.ContainsNewsFilter() && !_filterService.IndexFilter(FilteringConstants.FilterNews))
                {
                    _robotsSettings.Index = false;
                }
                else if (searchQuery.ContainsPriceFilter() && !_filterService.IndexFilter(FilteringConstants.FilterPrice))
                {
                    _robotsSettings.Index = false;
                }
            }

            if (searchQuery.PageNumber > 1)
            {
                _canonicalSettings.QueryStringParameters.Add(SearchQueryConstants.Page);
            }

            if (searchQuery.ContainsFilter() && !searchQuery.ContainsMultipleFilters())
            {
                if (indexFirstFilterName)
                {
                    _canonicalSettings.QueryStringParameters.Add("filter-" + firstFilterName);
                }
                else if (searchQuery.ContainsNewsFilter())
                {
                    _canonicalSettings.QueryStringParameters.Add(SearchQueryConstants.News);
                }
                else if (searchQuery.ContainsPriceFilter())
                {
                    _canonicalSettings.QueryStringParameters.Add(SearchQueryConstants.FromPrice);
                    _canonicalSettings.QueryStringParameters.Add(SearchQueryConstants.ToPrice);
                    _canonicalSettings.QueryStringParameters.Add(SearchQueryConstants.PriceRange);
                }
            }
        }
    }
}
