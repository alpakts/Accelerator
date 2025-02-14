using System;
using Litium.Accelerator.Builders.Home;
using Litium.Security;
using Litium.Web.Models.Websites;
using Litium.Web.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Home
{
    public class HomeController : ControllerBase
    {
        private readonly HomeViewModelBuilder _builder;
        private readonly AuthorizationService _authorizationService;
        private readonly RouteRequestLookupInfoAccessor _routeRequestLookupInfoAccessor;

        public HomeController(
            HomeViewModelBuilder builder,
            AuthorizationService authorizationService,
            RouteRequestLookupInfoAccessor routeRequestLookupInfoAccessor)
        {
            _builder = builder;
            _authorizationService = authorizationService;
            _routeRequestLookupInfoAccessor = routeRequestLookupInfoAccessor;
        }

        public IActionResult Index(PageModel currentPageModel)
        {
            if (_routeRequestLookupInfoAccessor.RouteRequestLookupInfo?.PreviewPageData?.GlobalBlockSystemId is Guid previewGlobalBlock
                && _authorizationService.HasOperation(Operations.Function.Websites.UI))
            {
                return View("Index", _builder.ForPreviewGlobalBlock(previewGlobalBlock));
            }
            return View(_builder.Build(currentPageModel));
        }
    }
}
