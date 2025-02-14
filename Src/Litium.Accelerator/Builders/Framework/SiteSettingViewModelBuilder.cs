using Litium.Accelerator.Routing;
using Litium.Accelerator.ViewModels.Framework;
using Litium.Web.Products.Routing;
using Litium.Web.Routing;

namespace Litium.Accelerator.Builders.Framework
{
    /// <summary>
    /// Represents the builder for <see cref="SiteSettingViewModel"/>, which contains current website setting.
    /// </summary>
    public class SiteSettingViewModelBuilder : IViewModelBuilder<SiteSettingViewModel>
    {
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly RouteRequestInfoAccessor _routeRequestInfoAccessor;
        private readonly RouteRequestLookupInfoAccessor _routeRequestLookupInfoAccessor;

        public SiteSettingViewModelBuilder(RequestModelAccessor requestModelAccessor ,RouteRequestInfoAccessor routeRequestInfoAccessor, RouteRequestLookupInfoAccessor routeRequestLookupInfoAccessor)
        {
            _requestModelAccessor = requestModelAccessor;
            _routeRequestInfoAccessor = routeRequestInfoAccessor;
            _routeRequestLookupInfoAccessor = routeRequestLookupInfoAccessor;
        }

        public SiteSettingViewModel Build()
        {
            var productPageData = _routeRequestInfoAccessor.RouteRequestInfo.Data as ProductPageData;
            return new SiteSettingViewModel()
            {
                ChannelSystemId = _requestModelAccessor.RequestModel.ChannelModel.SystemId,
                CurrentPageSystemId = _requestModelAccessor.RequestModel.CurrentPageModel.SystemId,
                ProductCategorySystemId = productPageData?.CategorySystemId,
                PreviewPageData = _routeRequestLookupInfoAccessor.RouteRequestLookupInfo?.PreviewPageData
            };
        }
    }
}
