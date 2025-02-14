using System.Web;
using Litium.Accelerator.Caching;
using Litium.Runtime.DependencyInjection;
using Litium.Web;
using Litium.Web.Routing;

namespace Litium.Accelerator.Mvc.Routing
{
    [ServiceDecorator(typeof(ISignInUrlResolver))]
    internal class SignInUrlResolverDecorator : ISignInUrlResolver
    {
        private readonly UrlService _urlService;
        private readonly PageByFieldTemplateCache<LoginPageByFieldTemplateCache> _pageByFieldType;
        private readonly ISignInUrlResolver _parentResolver;

        public SignInUrlResolverDecorator(
            ISignInUrlResolver parentResolver,
            UrlService urlService,
            PageByFieldTemplateCache<LoginPageByFieldTemplateCache> pageByFieldType)
        {
            _parentResolver = parentResolver;
            _urlService = urlService;
            _pageByFieldType = pageByFieldType;
        }

        public bool TryGet(RouteRequestLookupInfo routeRequestLookupInfo, out string redirectUrl)
        {
            string resultUrl = null;
            if (_pageByFieldType.TryFindPage(page =>
            {
                var url = _urlService.GetUrl(page, new PageUrlArgs(routeRequestLookupInfo.Channel.SystemId));
                if (url == null)
                {
                    return false;
                }
                resultUrl = string.Concat(url, url.Contains("?") ? "&" : "?", "RedirectUrl=", HttpUtility.UrlEncode(routeRequestLookupInfo.RawUrl));
                return true;
            }))
            {
                redirectUrl = resultUrl;
                return true;
            }

            return _parentResolver.TryGet(routeRequestLookupInfo, out redirectUrl);
        }
    }
}
