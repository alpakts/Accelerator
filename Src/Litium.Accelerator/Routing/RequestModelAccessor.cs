using System.Threading;
using Litium.Runtime.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Litium.Accelerator.Routing
{
    [Service(ServiceType = typeof(RequestModelAccessor))]
    public class RequestModelAccessor
    {
        private static readonly AsyncLocal<RequestModel> _routeRequest = new AsyncLocal<RequestModel>();
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestModelAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public virtual RequestModel RequestModel
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if (context != null)
                {
                    return (RequestModel)context.Items[nameof(RequestModel)];
                }

                return _routeRequest.Value;
            }

            set
            {
                var context = _httpContextAccessor.HttpContext;
                if (context != null)
                {
                    context.Items[nameof(RequestModel)] = value;
                }
                else
                {
                    _routeRequest.Value = value;
                }
            }
        }
    }
}
