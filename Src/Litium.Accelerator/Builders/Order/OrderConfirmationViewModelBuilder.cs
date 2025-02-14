using System;
using Litium.Accelerator.ViewModels.Order;
using Litium.Runtime.AutoMapper;
using Litium.Sales;
using Litium.Web.Models.Websites;

namespace Litium.Accelerator.Builders.Order
{
    public class OrderConfirmationViewModelBuilder : IViewModelBuilder<OrderConfirmationViewModel>
    {
        private readonly OrderViewModelBuilder _orderViewModelBuilder;
        private readonly OrderOverviewService _orderOverviewService;
        private readonly CartContextAccessor _cartContextAccessor;

        public OrderConfirmationViewModelBuilder(
            OrderViewModelBuilder orderViewModelBuilder,
            OrderOverviewService orderOverviewService,
            CartContextAccessor cartContextAccessor)
        {
            _orderViewModelBuilder = orderViewModelBuilder;
            _orderOverviewService = orderOverviewService;
            _cartContextAccessor = cartContextAccessor;
        }

        public OrderConfirmationViewModel Build(PageModel pageModel, Guid orderSystemId)
        {
            var model = pageModel.MapTo<OrderConfirmationViewModel>();
            var order = _orderOverviewService.Get(orderSystemId);
            if (order != null)
            {
                model.Order = _orderViewModelBuilder.Build(order);
            }
            return model;
        }
    }
}
