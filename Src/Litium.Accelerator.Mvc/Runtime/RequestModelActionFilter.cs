using System;
using System.Globalization;
using System.Linq;
using System.Net;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Search;
using Litium.FieldFramework;
using Litium.Globalization;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Sales;
using Litium.Web;
using Litium.Web.Models.Globalization;
using Litium.Web.Models.Products;
using Litium.Web.Models.Websites;
using Litium.Web.Mvc.Runtime;
using Litium.Web.Products.Routing;
using Litium.Web.Routing;
using Litium.Websites;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Litium.Accelerator.Mvc.Runtime
{
    public class RequestModelActionFilter : IActionFilter, IResultFilter
    {
        private readonly RouteRequestLookupInfoAccessor _routeRequestLookupInfoAccessor;
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly RouteRequestInfoAccessor _routeRequestInfoAccessor;
        private readonly ISecureConnectionResolver _secureConnectionResolver;
        private readonly ChannelService _channelService;
        private readonly DomainNameService _domainNameService;
        private readonly LanguageService _languageService;
        private readonly PageService _pageService;
        private readonly FieldTemplateService _fieldTemplateService;
        private readonly UrlService _urlService;
        private readonly CountryService _countryService;

        public RequestModelActionFilter(
            RouteRequestLookupInfoAccessor routeRequestLookupInfoAccessor,
            RequestModelAccessor requestModelAccessor,
            RouteRequestInfoAccessor routeRequestInfoAccessor,
            ISecureConnectionResolver secureConnectionResolver,
            ChannelService channelService,
            DomainNameService domainNameService,
            LanguageService languageService,
            PageService pageService,
            FieldTemplateService fieldTemplateService,
            UrlService urlService,
            CountryService countryService)
        {
            _routeRequestLookupInfoAccessor = routeRequestLookupInfoAccessor;
            _requestModelAccessor = requestModelAccessor;
            _routeRequestInfoAccessor = routeRequestInfoAccessor;
            _secureConnectionResolver = secureConnectionResolver;
            _channelService = channelService;
            _domainNameService = domainNameService;
            _languageService = languageService;
            _pageService = pageService;
            _fieldTemplateService = fieldTemplateService;
            _urlService = urlService;
            _countryService = countryService;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var routeRequestLookupInfo = _routeRequestLookupInfoAccessor.RouteRequestLookupInfo;
            if (routeRequestLookupInfo != null)
            {
                _requestModelAccessor.RequestModel = new RequestModelImpl(filterContext.HttpContext.GetCartContext(), _countryService)
                {
                    _channelModel = new Lazy<ChannelModel>(() => routeRequestLookupInfo.Channel.MapTo<ChannelModel>()),
                    _searchQuery = new Lazy<SearchQuery>(() => filterContext.HttpContext.MapTo<SearchQuery>()),
                    _currentPageModel = new Lazy<PageModel>(() => _routeRequestInfoAccessor.RouteRequestInfo.PageSystemId.MapTo<PageModel>()),
                    _currentCategoryModel = new Lazy<CategoryModel>(() => (_routeRequestInfoAccessor.RouteRequestInfo?.Data as ProductPageData)?.CategorySystemId.MapTo<CategoryModel>()),
                    _currentProductModel = new Lazy<ProductModel>(() 
                        => (_routeRequestInfoAccessor.RouteRequestInfo?.Data as ProductPageData)?.VariantSystemId.MapTo<Variant>()?.MapTo<ProductModel>()
                        ?? (_routeRequestInfoAccessor.RouteRequestInfo?.Data as ProductPageData)?.BaseProductSystemId.MapTo<BaseProduct>()?.MapTo<ProductModel>()),
                };
            }
            else
            {
                var siteSettingViewModel = filterContext.HttpContext.Request.Headers.GetSiteSettingViewModel();
                if (siteSettingViewModel != null)
                {
                    var url = filterContext.HttpContext.Request.ToUri();
                    var channel = _channelService.Get(siteSettingViewModel.ChannelSystemId);
                    var domainNameLink = channel?.DomainNameLinks.FirstOrDefault();

                    _routeRequestLookupInfoAccessor.RouteRequestLookupInfo = new RouteRequestLookupInfo
                    {
                        AbsolutePath = WebUtility.UrlDecode(url.AbsolutePath),
                        IsSecureConnection = _secureConnectionResolver.IsUsingSecureConnection(),
                        QueryString = new QueryCollection(filterContext.HttpContext.Request),
                        RawUrl = url.PathAndQuery,
                        Uri = url,
                        Channel = channel,
                        DomainNameLink = domainNameLink,
                        DomainName = _domainNameService.Get(domainNameLink?.DomainNameSystemId ?? Guid.Empty),
                        IsInAdministration = siteSettingViewModel.PreviewPageData != null,
                        PreviewPageData = siteSettingViewModel.PreviewPageData
                    };

                    if (siteSettingViewModel.CurrentPageSystemId != Guid.Empty)
                    {
                        var page = _pageService.Get(siteSettingViewModel.CurrentPageSystemId);
                        if (page != null)
                        {
                            var fieldTemplate = _fieldTemplateService.Get<PageFieldTemplate>(page.FieldTemplateSystemId);
                            _routeRequestInfoAccessor.RouteRequestInfo = new RouteRequestInfo
                            {
                                PageSystemId = siteSettingViewModel.CurrentPageSystemId,
                                TemplateFileName = fieldTemplate.TemplatePath,
                                DataPath = _urlService.GetUrl(page, new PageUrlArgs(channel.SystemId))
                            };
                        }
                    }

                    if (siteSettingViewModel.ProductCategorySystemId != null)
                    {
                        if (!(_routeRequestInfoAccessor.RouteRequestInfo.Data is ProductPageData productData))
                        {
                            _routeRequestInfoAccessor.RouteRequestInfo.Data = productData = new ProductPageData();
                        }
                        productData.CategorySystemId = siteSettingViewModel.ProductCategorySystemId.Value;
                    }

                    _requestModelAccessor.RequestModel = new RequestModelImpl(filterContext.HttpContext.GetCartContext(), _countryService)
                    {
                        _channelModel = new Lazy<ChannelModel>(() => channel.MapTo<ChannelModel>()),
                        _searchQuery = new Lazy<SearchQuery>(() => filterContext.HttpContext.MapTo<SearchQuery>()),
                        _currentPageModel = new Lazy<PageModel>(() => siteSettingViewModel.CurrentPageSystemId.MapTo<PageModel>()),
                        _currentCategoryModel = new Lazy<CategoryModel>(() => (_routeRequestInfoAccessor.RouteRequestInfo?.Data as ProductPageData)?.CategorySystemId.MapTo<CategoryModel>()),
                        _currentProductModel = new Lazy<ProductModel>(()
                            => (_routeRequestInfoAccessor.RouteRequestInfo?.Data as ProductPageData)?.VariantSystemId.MapTo<Variant>()?.MapTo<ProductModel>()
                            ?? (_routeRequestInfoAccessor.RouteRequestInfo?.Data as ProductPageData)?.BaseProductSystemId.MapTo<BaseProduct>()?.MapTo<ProductModel>()),
                    };

                    CultureInfo.CurrentUICulture = _languageService.Get(channel.WebsiteLanguageSystemId.GetValueOrDefault())?.CultureInfo;
                    CultureInfo.CurrentCulture = _languageService.Get(channel.ProductLanguageSystemId.GetValueOrDefault())?.CultureInfo ?? CultureInfo.CurrentUICulture;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            _requestModelAccessor.RequestModel = null;
        }

        private class RequestModelImpl : RequestModel
        {
            public Lazy<ChannelModel> _channelModel;
            public Lazy<SearchQuery> _searchQuery;
            public Lazy<PageModel> _currentPageModel;
            public Lazy<ProductModel> _currentProductModel;
            public Lazy<CategoryModel> _currentCategoryModel;

            public RequestModelImpl(
                CartContext cartContext,
                CountryService countryService)
                : base(
                      cartContext,
                      countryService)
            {
            }

            public override ChannelModel ChannelModel => _channelModel?.Value;
            public override SearchQuery SearchQuery => _searchQuery?.Value;
            public override PageModel CurrentPageModel => _currentPageModel?.Value;
            public override ProductModel CurrentProductModel => _currentProductModel?.Value;
            public override CategoryModel CurrentCategoryModel => _currentCategoryModel?.Value;
        }
    }
}
