using System;
using Litium.Accelerator.Utilities;
using Litium.Web.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Litium.Accelerator.Mvc.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class OrganizationRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly bool _orderApproval;
        private readonly bool _orderPlacer;

        public OrganizationRoleAttribute(bool orderApproval, bool orderPlacer)
        {
            _orderApproval = orderApproval;
            _orderPlacer = orderPlacer;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var routeRequestLookupInfo = context.HttpContext.RequestServices.GetRequiredService<RouteRequestLookupInfoAccessor>().RouteRequestLookupInfo;
            if (routeRequestLookupInfo != null && routeRequestLookupInfo.IsInAdministration)
            {
                return;
            }

            var personStorage = context.HttpContext.RequestServices.GetRequiredService<PersonStorage>();
            if (_orderApproval && personStorage.HasApproverRole)
            {
                return;
            }

            if (_orderPlacer && personStorage.HasPlacerRole)
            {
                return;
            }

            context.Result = new ForbidResult();
        }
    }
}
