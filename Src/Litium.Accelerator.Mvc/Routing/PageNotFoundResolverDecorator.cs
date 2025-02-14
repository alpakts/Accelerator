using Litium.Accelerator.Caching;
using Litium.Runtime.DependencyInjection;
using Litium.Web;
using Litium.Web.Routing;

namespace Litium.Accelerator.Mvc.Routing
{
    [ServiceDecorator(typeof(IPageNotFoundResolver))]
    internal class PageNotFoundResolverDecorator : IPageNotFoundResolver
    {
        private readonly UrlService _urlService;
        private readonly PageByFieldTemplateCache<PageNotFoundByFieldTemplateCache> _pageByFieldType;
        private readonly IPageNotFoundResolver _parentResolver;

        public PageNotFoundResolverDecorator(
             IPageNotFoundResolver parentResolver,
             UrlService urlService,
             PageByFieldTemplateCache<PageNotFoundByFieldTemplateCache> pageByFieldType)
        {
            _parentResolver = parentResolver;
            _urlService = urlService;
            _pageByFieldType = pageByFieldType;
        }

        public bool TryGet(RouteRequestLookupInfo routeRequestLookupInfo, out RouteRequestInfo routeRequestInfo)
        {
            var result = new RouteRequestInfo();
            if (_pageByFieldType.TryFindPage(page =>
            {
                var url = _urlService.GetUrl(page, new PageUrlArgs(routeRequestLookupInfo.Channel.SystemId));
                if (url is null)
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
