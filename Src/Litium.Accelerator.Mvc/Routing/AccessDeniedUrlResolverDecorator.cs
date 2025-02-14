using Litium.Runtime.DependencyInjection;
using Litium.Web.Routing;

namespace Litium.Accelerator.Mvc.Routing
{
    [ServiceDecorator(typeof(IAccessDeniedUrlResolver))]
    internal class AccessDeniedUrlResolverDecorator : IAccessDeniedUrlResolver
    {
        private readonly IAccessDeniedUrlResolver _parent;
        private readonly ISignInUrlResolver _signInUrlResolver;

        public AccessDeniedUrlResolverDecorator(
            IAccessDeniedUrlResolver parent,
            ISignInUrlResolver signInUrlResolver)
        {
            _parent = parent;
            _signInUrlResolver = signInUrlResolver;
        }

        public bool TryGet(RouteRequestLookupInfo routeRequestLookupInfo, out string redirectUrl)
        {
            if (_signInUrlResolver.TryGet(routeRequestLookupInfo, out redirectUrl))
            {
                redirectUrl += (redirectUrl.Contains("?") ? "&" : "?") + "code=403";
                return true;
            }

            return _parent.TryGet(routeRequestLookupInfo, out redirectUrl);
        }
    }
}
