using System;
using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.Accelerator.ViewModels.Home;
using Litium.Runtime.AutoMapper;
using Litium.Web;
using Litium.Web.Models.Blocks;
using Litium.Web.Models.Websites;
using Litium.Web.Routing;

namespace Litium.Accelerator.Builders.Home
{
    public class HomeViewModelBuilder : IViewModelBuilder<HomeViewModel>
    {
        private readonly RouteRequestLookupInfoAccessor _routeRequestLookupInfoAccessor;

        public HomeViewModelBuilder(RouteRequestLookupInfoAccessor routeRequestLookupInfoAccessor)
        {
            _routeRequestLookupInfoAccessor = routeRequestLookupInfoAccessor;
        }

        public virtual HomeViewModel Build(PageModel pageModel)
        {
            return pageModel.MapTo<HomeViewModel>();
        }

        public virtual HomeViewModel ForPreviewGlobalBlock(Guid blockId)
        {
            var routeRequest = _routeRequestLookupInfoAccessor.RouteRequestLookupInfo;
            routeRequest.PreviewPageData = routeRequest.PreviewPageData ??
                                            new PreviewPageUrlArgs(routeRequest.Channel.SystemId);
            routeRequest.PreviewPageData.SkipRenderingValidation = true;
            return new HomeViewModel()
            {
                Blocks = new Dictionary<string, List<BlockModel>>()
                {
                    { BlockContainerNameConstant.Main, new List<BlockModel>() { blockId.MapTo<BlockModel>() } },
                }
            };
        }
    }
}
