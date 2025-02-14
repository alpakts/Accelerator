using System;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Builders.Menu;
using Litium.Globalization;
using Litium.Sales;
using Litium.Web.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Litium.Accelerator.Mvc.Controllers
{
    /// <summary>
    /// Controller base class that helps out to set correct layout for rendering.
    /// </summary>
    /// <seealso cref="Controller" />
    public abstract class ControllerBase : Controller
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cartContext = context.HttpContext.GetCartContext();
            if (cartContext != null && cartContext.Cart.SystemId != Guid.Empty)
            {
                var routeRequestLookupInto = context.HttpContext.RequestServices.GetRequiredService<RouteRequestLookupInfoAccessor>().RouteRequestLookupInfo;

                var channel = routeRequestLookupInto.Channel;
                if (cartContext.ChannelSystemId != channel.SystemId)
                {
                    await cartContext.SelectChannelAsync(new SelectChannelArgs
                    {
                        ChannelSystemId = channel.SystemId
                    });

                    //if cart has no country then using the first country link of request lookup channel
                    if (string.IsNullOrEmpty(cartContext.CountryCode))
                    {
                        var countryId = channel.CountryLinks.FirstOrDefault()?.CountrySystemId;
                        if (countryId.HasValue)
                        {
                            var countryService = context.HttpContext.RequestServices.GetRequiredService<CountryService>();
                            var countryCode = countryService.Get(countryId.Value)?.Id;
                            if (countryCode is object)
                            {
                                await cartContext.SelectCountryAsync(new SelectCountryArgs
                                {
                                    CountryCode = countryCode
                                });
                            }
                        }
                    }
                }
            }
            await next();
        }

        [NonAction]
        public ViewResult View(string viewName, string masterName, object model)
        {
            var menuViewModelBuilder = HttpContext.RequestServices.GetRequiredService<MenuViewModelBuilder>();
            if (string.IsNullOrEmpty(masterName))
            {
                var menuModel = menuViewModelBuilder.Build();
                masterName = menuModel.ShowLeftColumn
                    ? "~/Views/Shared/_LayoutWithLeftColumn.cshtml"
                    : "~/Views/Shared/_Layout.cshtml";
            }

            ViewData["MasterName"] = masterName;

            return base.View(viewName, model);
        }

        public override ViewResult View(string viewName, object model)
            => View(viewName, null, model);
    }
}
