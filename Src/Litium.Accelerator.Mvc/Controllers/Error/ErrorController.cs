using Litium.Web.Models.Websites;
using Litium.Web.Routing;
using System.Net;
using System.Security.Cryptography;
using Litium.Accelerator.Builders.Error;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Litium.Accelerator.Services;

namespace Litium.Accelerator.Mvc.Controllers.Error
{
    public class ErrorController : ControllerBase
    {
        private readonly ErrorViewModelBuilder _errorViewModelBuilder;
        private readonly RouteRequestLookupInfoAccessor _routeRequestLookupInfoAccessor;

        public ErrorController(ErrorViewModelBuilder errorViewModelBuilder, RouteRequestLookupInfoAccessor routeRequestLookupInfoAccessor)
        {
            _errorViewModelBuilder = errorViewModelBuilder;
            _routeRequestLookupInfoAccessor = routeRequestLookupInfoAccessor;
        }

        [HttpGet]
        public async Task<ActionResult> Error(PageModel currentPageModel)
        {
            if (!_routeRequestLookupInfoAccessor.RouteRequestLookupInfo.IsInAdministration)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Add random sleep due to Microsoft Security Advisory (2416728), obfuscate error time
                var delay = new byte[1];
                using (RandomNumberGenerator prng = RandomNumberGenerator.Create())
                {
                    prng.GetBytes(delay);
                }
                await Task.Delay(delay[0]);
            }
            var model = _errorViewModelBuilder.Build(currentPageModel);

            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionFeature?.Error is ErrorPageMoreInfoException ex)
            {
                model.Text = ex.Message;
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult NotFound(PageModel currentPageModel)
        {
            if (!_routeRequestLookupInfoAccessor.RouteRequestLookupInfo.IsInAdministration)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            var model = _errorViewModelBuilder.Build(currentPageModel);
            return View(model);
        }
    }
}
