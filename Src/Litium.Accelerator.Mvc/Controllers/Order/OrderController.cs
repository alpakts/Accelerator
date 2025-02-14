using System;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Builders.Checkout;
using Litium.Accelerator.Builders.Order;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Services;
using Litium.Sales;
using Litium.Web;
using Litium.Web.Models.Websites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Litium.Accelerator.Mvc.Controllers.Order
{
    public class OrderController : ControllerBase
    {
        private readonly OrderViewModelBuilder _orderViewModelBuilder;
        private readonly OrderConfirmationViewModelBuilder _orderConfirmationViewModelBuilder;
        private readonly PaymentOptionViewModelBuilder _paymentOptionViewModelBuilder;
        private readonly ILogger<OrderController> _logger;
        private readonly string _businessCustomerOrderView = "BusinessCustomerOrder";
        private readonly string _orderConfirmationEmailView = "../Mail/ConfirmationEmail";

        public OrderController(
            OrderViewModelBuilder orderViewModelBuilder,
            OrderConfirmationViewModelBuilder orderConfirmationViewModelBuilder,
            PaymentOptionViewModelBuilder paymentOptionViewModelBuilder,
            ILogger<OrderController> logger)
        {
            _orderViewModelBuilder = orderViewModelBuilder;
            _orderConfirmationViewModelBuilder = orderConfirmationViewModelBuilder;
            _paymentOptionViewModelBuilder = paymentOptionViewModelBuilder;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult Order(Guid id, bool print = false)
        {
            var model = _orderViewModelBuilder.Build(id, print);
            if (model.IsBusinessCustomer)
            {
                return View(_businessCustomerOrderView, model);
            }
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Confirmation(PageModel currentPageModel, Guid? orderId, bool isEmail = false)
        {
            if (isEmail)
            {
                if (orderId.HasValue && orderId.Value != Guid.Empty)
                {
                    // In case of sending the order confirmation email, we get the orderId from the query string
                    var model = _orderConfirmationViewModelBuilder.Build(currentPageModel, orderId.Value);
                    return View(_orderConfirmationEmailView, model);
                }
                else
                {
                    _logger.LogError("An error occured while trying to render order confirmation email, order id is missing in request");
                    return View(_orderConfirmationEmailView);
                }
            }
            else
            {
                var cartContext = HttpContext.GetCartContext();
                try
                {
                    if (orderId == default
                        && cartContext.PaymentFlowResults.Count > 0
                        && currentPageModel.GetValue<bool>(CheckoutPageFieldNameConstants.UseConfirmationWidget))
                    {
                        var firstPaymentOverview = cartContext.Cart.PaymentOverviews.FirstOrDefault();
                        var widgetResult = _paymentOptionViewModelBuilder.BuildWidget(cartContext, firstPaymentOverview.Payment.PaymentOption, isReceipt: true);
                        if (widgetResult?.ResponseString is not null)
                        {
                            return View("ConfirmationWidget", widgetResult.ResponseString);
                        }
                    }

                    var model = _orderConfirmationViewModelBuilder.Build(currentPageModel, orderId ?? cartContext.Cart.Order.SystemId);
                    if (model.Order.OrderId == Guid.Empty)
                    {
                        throw new Exception("Order does not exists.");
                    }

                    return View(model);
                }
                catch (Exception ex)
                {
                    const string errorKey = "confirmation.error";
                    var message = errorKey.AsWebsiteText();
                    if (message == errorKey)
                    {
                        message = "An error occurred while displaying the confirmation page, but we can confirm that your order has been placed.";
                    }

                    throw new ErrorPageMoreInfoException(message, ex);
                }
                finally
                {
                    await cartContext.ClearCartContextAsync();
                }
            }
        }
    }
}
