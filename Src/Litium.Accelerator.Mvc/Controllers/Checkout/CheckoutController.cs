using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Builders.Checkout;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Routing;
using Litium.FieldFramework.FieldTypes;
using Litium.Globalization;
using Litium.Runtime.AutoMapper;
using Litium.Sales;
using Litium.Sales.Payments.PaymentFlowActions;
using Litium.Validations;
using Litium.Web;
using Litium.Web.Models.Websites;
using Litium.Websites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Litium.Accelerator.Mvc.Controllers.Checkout
{
    public class CheckoutController : ControllerBase
    {
        private readonly CheckoutViewModelBuilder _checkoutViewModelBuilder;
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly PageService _pageService;
        private readonly UrlService _urlService;
        private readonly CountryService _countryService;
        private readonly PaymentService _paymentService;
        private readonly PaymentProviderService _paymentProviderService;
        private readonly ChannelService _channelService;
        private readonly WebsiteService _websiteService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            CheckoutViewModelBuilder checkoutViewModelBuilder,
            RequestModelAccessor requestModelAccessor,
            UrlService urlService,
            PageService pageService,
            CountryService countryService,
            PaymentService paymentService,
            PaymentProviderService paymentProviderService,
            ChannelService channelService,
            WebsiteService websiteService,
            ILogger<CheckoutController> logger)
        {
            _checkoutViewModelBuilder = checkoutViewModelBuilder;
            _requestModelAccessor = requestModelAccessor;
            _urlService = urlService;
            _pageService = pageService;
            _countryService = countryService;
            _paymentService = paymentService;
            _paymentProviderService = paymentProviderService;
            _channelService = channelService;
            _websiteService = websiteService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var cartContext = await HttpContext.GetCartContextAsync();

            var updateAddressAction = cartContext.PaymentFlowResults?
                .Select(r => r.PaymentFlowAction)
                .OfType<UpdateAddress>()
                .FirstOrDefault();
            if (updateAddressAction is not null && updateAddressAction.CountryCode != cartContext.CountryCode)
            {
                await cartContext.SelectCountryAsync(new SelectCountryArgs { CountryCode = updateAddressAction.CountryCode });
            }

            if (cartContext.Cart.Confirmed)
            {
                return Redirect(cartContext.CheckoutFlowInfo.ReceiptPageUrl);
            }

            if (!await cartContext.TryInitializeCheckoutFlowAsync(() => new CheckoutFlowInfoArgs
            {
                CheckoutFlowInfo = GetCheckoutFlowInfo()
            }))
            {
                // Initialization of the checkout flow have failed
                // To recover the user to not get stuck and not can
                // visit the checkout page again we will recreate the
                // cart context with.

                var discountCodes = cartContext.Cart.DiscountCodes;
                var giftcards = cartContext.Cart.GiftCards;
                var orderRows = cartContext.Cart.Order.Rows;

                await cartContext.ClearCartContextAsync();
                foreach (var row in orderRows.Where(x => x.OrderRowType == OrderRowType.Product))
                {
                    await cartContext.AddOrUpdateItemAsync(new AddOrUpdateCartItemArgs
                    {
                        AdditionalInfo = row.AdditionalInfo?.ToDictionary(s => s.Key, s => s.Value),
                        ArticleNumber = row.ArticleNumber,
                        Quantity = row.Quantity,
                        ConstantQuantity = true,
                        AlwaysAddItem = true,
                    });
                }

                foreach (var code in discountCodes)
                {
                    await cartContext.AddDiscountCodeAsync(code);
                }

                foreach (var giftcard in giftcards)
                {
                    await cartContext.AddGiftCardAsync(giftcard);
                }

                await cartContext.AddOrUpdateCheckoutFlowAsync(new CheckoutFlowInfoArgs
                {
                    CheckoutFlowInfo = GetCheckoutFlowInfo()
                });
            }

            var model = await _checkoutViewModelBuilder.BuildAsync(cartContext);

            try
            {
                await cartContext.ValidateAsync();
            }
            catch (ValidationException ex)
            {
                await ex.ProcessPostActionsAsync(cartContext);
                model.ErrorMessages.Add("general", new List<string> { ex.Message });
            }

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> PlaceOrderDirect()
        {
            var cartContext = await HttpContext.GetCartContextAsync();

            try
            {
                await cartContext.ValidateAsync();
            }
            catch (ValidationException ex)
            {
                await ex.ProcessPostActionsAsync(cartContext);
                await SetDefaultPaymentMethod(cartContext);
                var model = await _checkoutViewModelBuilder.BuildAsync(cartContext);
                model.ErrorMessages.Add("general", new List<string> { ex.Message });
                return View("Index", model);
            }

            try
            {
                await cartContext.ConfirmOrderAsync();
                return Redirect(GetOrderConfirmationPageUrl(cartContext.Cart.Order));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when placing an order");
                await ex.ProcessPostActionsAsync(cartContext);
                await SetDefaultPaymentMethod(cartContext);
                var model = await _checkoutViewModelBuilder.BuildAsync(cartContext);
                model.ErrorMessages.Add("general", new List<string> { "checkout.generalerror".AsWebsiteText() });
                return View("Index", model);
            }
        }

        private string GetAbsolutePageUrl(PointerPageItem pointer)
        {
            if (pointer == null)
            {
                return null;
            }
            var page = _pageService.Get(pointer.EntitySystemId);
            if (page == null)
            {
                return null;
            }

            var channelSystemId = pointer.ChannelSystemId != Guid.Empty ? pointer.ChannelSystemId : _requestModelAccessor.RequestModel.ChannelModel.SystemId;
            return _urlService.GetUrl(page, new PageUrlArgs(channelSystemId) { AbsoluteUrl = true });
        }

        public virtual async Task SetDefaultPaymentMethod(CartContext cartContext)
        {
            var country = _countryService.Get(cartContext?.CountryCode);
            var paymentOption = _requestModelAccessor.RequestModel.ChannelModel?.Channel?.CountryLinks?.FirstOrDefault(x => x.CountrySystemId == country?.SystemId)?.PaymentOptions.FirstOrDefault();
            if (paymentOption != null)
            {
                var paymentProvider = _paymentProviderService.Get(paymentOption.Id.ProviderId);
                var orderPaymentLink = _requestModelAccessor.RequestModel.Cart.Order.OrderPaymentLinks.FirstOrDefault();
                if (orderPaymentLink != null)
                {
                    var payment = _paymentService.Get(orderPaymentLink.PaymentSystemId);
                    // Set default payment option
                    if (payment != null && payment.PaymentOption.ProviderId != paymentProvider.Id && payment.PaymentOption.OptionId != paymentOption.Id.OptionId)
                    {
                        var selectPaymentArgs = new SelectPaymentOptionArgs
                        {
                            PaymentOptionId = paymentOption.Id
                        };
                        await cartContext.SelectPaymentOptionAsync(selectPaymentArgs);
                    }
                }
            }
        }

        private CheckoutFlowInfo GetCheckoutFlowInfo()
        {
            var websiteModel = _requestModelAccessor.RequestModel.WebsiteModel;
            var checkoutPage = websiteModel.GetValue<PointerPageItem>(AcceleratorWebsiteFieldNameConstants.CheckoutPage);
            var checkoutModeInt = websiteModel.GetValue<int?>(AcceleratorWebsiteFieldNameConstants.CheckoutMode) ?? 1;
            var checkoutPageUrl = GetAbsolutePageUrl(checkoutPage);

            return new CheckoutFlowInfo
            {
                AllowSeparateShippingAddress = true,
                CustomerType = ToCustomerType((CheckoutMode)checkoutModeInt),
                CheckoutPageUrl = checkoutPageUrl,
                TermsUrl = GetAbsolutePageUrl(checkoutPage?.EntitySystemId.MapTo<PageModel>().GetValue<PointerPageItem>(CheckoutPageFieldNameConstants.TermsAndConditionsPage)),
                ReceiptPageUrl = GetAbsolutePageUrl(websiteModel.GetValue<PointerPageItem>(AcceleratorWebsiteFieldNameConstants.OrderConfirmationPage)),
                CancelPageUrl = checkoutPageUrl,
            };

            static CustomerType? ToCustomerType(CheckoutMode checkoutMode) => checkoutMode switch
            {
                CheckoutMode.PrivateCustomers => CustomerType.Person,
                CheckoutMode.CompanyCustomers => CustomerType.Organization,
                CheckoutMode.Both => null,
                _ => throw new NotSupportedException("The checkout mode " + checkoutMode + " is not supported")
            };
        }

        private string GetOrderConfirmationPageUrl(SalesOrder order)
        {
            var channel = _channelService.Get(order.ChannelSystemId.GetValueOrDefault());
            var website = _websiteService.Get(channel.WebsiteSystemId.GetValueOrDefault());
            var pointerPage = website.Fields.GetValue<PointerPageItem>(AcceleratorWebsiteFieldNameConstants.OrderConfirmationPage);

            if (pointerPage == null)
            {
                throw new CheckoutException("Order is created, order confirmation page is missing.");
            }

            var channelSystemId = pointerPage.ChannelSystemId != Guid.Empty ? pointerPage.ChannelSystemId : order.ChannelSystemId.Value;
            var url = _urlService.GetUrl(_pageService.Get(pointerPage.EntitySystemId), new PageUrlArgs(channelSystemId));

            if (string.IsNullOrEmpty(url))
            {
                throw new CheckoutException("Order is created, order confirmation page is missing.");
            }

            return url;
        }
    }
}
