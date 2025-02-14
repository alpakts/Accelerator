using Litium.Accelerator.Caching;
using Litium.Runtime.DependencyInjection;
using Litium.Web;
using Litium.Web.Routing;

namespace Litium.Accelerator.Mvc.Routing
{
    [ServiceDecorator(typeof(IErrorPageResolver))]
    internal class ErrorPageResolverDecorator : IErrorPageResolver
    {
        private readonly UrlService _urlService;
        private readonly PageByFieldTemplateCache<ErrorPageByFieldTypeResolverType> _pageByFieldType;
        private readonly RouteRequestLookupInfoAccessor _routeRequestLookupInfoAccessor;
        private readonly IErrorPageResolver _parentResolver;

        public ErrorPageResolverDecorator(
             IErrorPageResolver parentResolver,
             UrlService urlService,
             PageByFieldTemplateCache<ErrorPageByFieldTypeResolverType> pageByFieldType,
             RouteRequestLookupInfoAccessor routeRequestLookupInfoAccessor)
        {
            _parentResolver = parentResolver;
            _urlService = urlService;
            _pageByFieldType = pageByFieldType;
            _routeRequestLookupInfoAccessor = routeRequestLookupInfoAccessor;
        }

        public bool TryGet(RouteRequestLookupInfo routeRequestLookupInfo, out RouteRequestInfo routeRequestInfo)
        {
            var result = new RouteRequestInfo();
            if (_pageByFieldType.TryFindPage(page =>
            {
                var url = _urlService.GetUrl(page, new PageUrlArgs(routeRequestLookupInfo.Channel.SystemId));
                if (url == null)
                {
                    return false;
                }

                result.PageSystemId = page.SystemId;
                result.DataPath = url;
                return true;
            }, routeRequestLookupInfo: routeRequestLookupInfo))
            {
                routeRequestInfo = result;
                return true;
            }

            return _parentResolver.TryGet(routeRequestLookupInfo, out routeRequestInfo);
        }
    }
}
