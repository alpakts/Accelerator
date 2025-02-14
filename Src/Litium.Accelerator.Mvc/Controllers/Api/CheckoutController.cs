using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Builders.Checkout;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Mailing.Models;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Services;
using Litium.Accelerator.Utilities;
using Litium.Accelerator.ViewModels.Checkout;
using Litium.Accelerator.ViewModels.Persons;
using Litium.Customers;
using Litium.FieldFramework;
using Litium.FieldFramework.FieldTypes;
using Litium.Globalization;
using Litium.Runtime.AutoMapper;
using Litium.Sales;
using Litium.Security;
using Litium.Validations;
using Litium.Web;
using Litium.Websites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Litium.Accelerator.Mvc.Controllers.Api
{
    [Route("api/checkout")]
    public class CheckoutController : ApiControllerBase
    {
        private readonly SecurityContextService _securityContextService;
        private readonly FieldTemplateService _templateService;
        private readonly PersonService _personService;
        private readonly OrganizationService _organizationService;
        private readonly AddressTypeService _addressTypeService;
        private readonly ChannelService _channelService;
        private readonly WebsiteService _websiteService;
        private readonly PageService _pageService;
        private readonly UrlService _urlService;
        private readonly UserValidationService _userValidationService;
        private readonly LoginService _loginService;
        private readonly MailService _mailService;
        private readonly WelcomeEmailDefinitionResolver _welcomeEmailDefinitionResolver;
        private readonly PaymentOptionViewModelBuilder _paymentOptionViewModelBuilder;
        private readonly DeliveryMethodViewModelBuilder _deliveryMethodViewModelBuilder;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            SecurityContextService securityContextService,
            FieldTemplateService templateService,
            PersonService personService,
            OrganizationService organizationService,
            AddressTypeService addressTypeService,
            ChannelService channelService,
            WebsiteService websiteService,
            PageService pageService,
            UrlService urlService,
            UserValidationService userValidationService,
            LoginService loginService,
            MailService mailService,
            WelcomeEmailDefinitionResolver welcomeEmailDefinitionResolver,
            PaymentOptionViewModelBuilder paymentOptionViewModelBuilder,
            DeliveryMethodViewModelBuilder deliveryMethodViewModelBuilder,
            ILogger<CheckoutController> logger)
        {
            _securityContextService = securityContextService;
            _templateService = templateService;
            _personService = personService;
            _organizationService = organizationService;
            _addressTypeService = addressTypeService;
            _channelService = channelService;
            _websiteService = websiteService;
            _pageService = pageService;
            _urlService = urlService;

            _userValidationService = userValidationService;
            _loginService = loginService;
            _mailService = mailService;
            _welcomeEmailDefinitionResolver = welcomeEmailDefinitionResolver;
            _paymentOptionViewModelBuilder = paymentOptionViewModelBuilder;
            _deliveryMethodViewModelBuilder = deliveryMethodViewModelBuilder;
            _logger = logger;
        }

        /// <summary>
        /// Submits the current shopping cart and places the order.
        /// </summary>
        /// <param name="model">Object containing all information of the order including delivery info and payment info.</param>
        [Route("")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            if (!Validate(ModelState, model))
            {
                return BadRequest(ModelState);
            }

            var cartContext = await HttpContext.GetCartContextAsync();

            try
            {
                //if shipping address is empty, using billing address as shipping address
                var shippingAddress = cartContext.Cart.Order.ShippingInfo.FirstOrDefault()?.ShippingAddress;
                if (!model.ShowAlternativeAddress || shippingAddress is null || shippingAddress.IsEmpty())
                {
                    await cartContext.AddOrUpdateDeliveryAddressAsync(cartContext.Cart.Order.BillingAddress);
                }

                await UpdateOrderCommentAsync(model, cartContext);

                var userId = _securityContextService.GetIdentityUserSystemId();
                if (userId is not null && userId != SecurityContextService.Everyone.SystemId)
                {
                    var person = _personService.Get(userId.Value).MakeWritableClone();
                    if (person != null)
                    {
                        UpdatePersonInformation(model, cartContext, person);
                    }
                }
                else if (model.SignUp && !model.IsBusinessCustomer)
                {
                    await RegisterNewUserAsync(model, cartContext);
                }

                await cartContext.ConfirmOrderAsync();

                model.RedirectUrl = GetOrderConfirmationPageUrl(cartContext.Cart.Order);
                return Ok(model);
            }
            catch (ValidationException ex)
            {
                await ex.ProcessPostActionsAsync(cartContext);
                ModelState.AddModelError("general", ex.Message);
                return BadRequest(ModelState);
            }
            catch (CheckoutException ex)
            {
                ModelState.AddModelError("general", ex.Message);
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when placing an order");
                ModelState.AddModelError("general", "checkout.generalerror".AsWebsiteText());
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates the payment method on the current shopping cart.
        /// </summary>
        /// <param name="model">Object containing the selected payment method.</param>
        [HttpPut]
        [ValidateAntiForgeryToken]
        [Route("setPaymentProvider")]
        public async Task<IActionResult> SetPaymentProvider(CheckoutViewModel model)
        {
            var selectedPaymentMethod = model.SelectedPaymentMethod;
            if (selectedPaymentMethod is null || string.IsNullOrEmpty(selectedPaymentMethod.Id))
            {
                ModelState.AddModelError("payment", "checkout.setpaymenterror".AsWebsiteText());
                return BadRequest(ModelState);
            }

            try
            {

                var selectPaymentArgs = new SelectPaymentOptionArgs
                {
                    PaymentOptionId = selectedPaymentMethod.Id
                };
                var cartContext = await HttpContext.GetCartContextAsync();
                await cartContext.SelectPaymentOptionAsync(selectPaymentArgs);
                model.PaymentWidget = _paymentOptionViewModelBuilder.BuildWidget(cartContext, selectedPaymentMethod.Id);
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when changing payment method to {SelectedPaymentMethod}", selectedPaymentMethod.Id);
                ModelState.AddModelError("payment", "checkout.setpaymenterror".AsWebsiteText());
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates the delivery method on the current shopping cart.
        /// </summary>
        /// <param name="model">Object containing the selected delivery method.</param>
        [HttpPut]
        [ValidateAntiForgeryToken]
        [Route("setDeliveryProvider")]
        public async Task<IActionResult> SetDeliveryProvider(CheckoutViewModel model)
        {
            var selectedDeliveryMethod = model.SelectedDeliveryMethod;
            if (selectedDeliveryMethod is null || string.IsNullOrEmpty(selectedDeliveryMethod.Id))
            {
                ModelState.AddModelError("general", "checkout.setdeliveryerror".AsWebsiteText());
                return BadRequest(ModelState);
            }

            try
            {
                var selectShippingOptionArgs = new SelectShippingOptionArgs
                {
                    ShippingOptionId = selectedDeliveryMethod.Id
                };
                var cartContext = await HttpContext.GetCartContextAsync();
                await cartContext.SelectShippingOptionAsync(selectShippingOptionArgs);

                model.ShippingInfo = default;
                if (model.PaymentWidget != null)
                {
                    model.PaymentWidget = _paymentOptionViewModelBuilder.BuildWidget(cartContext, model.SelectedPaymentMethod.Id);
                }
                model.ShipmentWidget = _deliveryMethodViewModelBuilder.BuildWidget(cartContext, model.SelectedDeliveryMethod.Id);

                // select the first delivery group option.    
                var firstDeliveryOptionsGroup = cartContext.ShippingFlowResults.FirstOrDefault()?.DeliveryOptionGroups.FirstOrDefault();
                if (firstDeliveryOptionsGroup?.DeliveryOptions?.Count == 1)
                {
                    await cartContext.UpdateShippingSessionAsync(new UpdateShippingSessionArgs() { UpdateSessionData = firstDeliveryOptionsGroup?.DeliveryOptions?.FirstOrDefault().Id });
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when changing delivery method to {SelectedDeliveryMethod}", selectedDeliveryMethod.Id);
                ModelState.AddModelError("general", "checkout.setdeliveryerror".AsWebsiteText());
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Reload payment widget when update item in cart.
        /// </summary>
        /// <param name="model">Object containing the selected delivery method.</param>
        [HttpPut]
        [ValidateAntiForgeryToken]
        [Route("reloadPaymentWidget")]
        public async Task<IActionResult> ReloadPaymentWidget(CheckoutViewModel model)
        {
            if (model.SelectedPaymentMethod is null || string.IsNullOrEmpty(model.SelectedPaymentMethod.Id))
            {
                ModelState.AddModelError("general", "checkout.reloadpaymentwidgeterror".AsWebsiteText());
                return BadRequest(ModelState);
            }

            try
            {
                var cartContext = await HttpContext.GetCartContextAsync();
                if (model.PaymentWidget != null)
                {
                    model.PaymentWidget = _paymentOptionViewModelBuilder.BuildWidget(cartContext, model.SelectedPaymentMethod.Id);
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when reload payment widget");
                ModelState.AddModelError("general", "checkout.reloadpaymentwidgeterror".AsWebsiteText());
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Reload delivery widget when update item in cart.
        /// </summary>
        /// <param name="model">Object containing the selected delivery method.</param>
        [HttpPut]
        [ValidateAntiForgeryToken]
        [Route("reloadShipmentWidget")]
        public async Task<IActionResult> ReloadShipmentWidget(CheckoutViewModel model)
        {
            if (model.SelectedDeliveryMethod is null || string.IsNullOrEmpty(model.SelectedDeliveryMethod.Id))
            {
                ModelState.AddModelError("general", "checkout.reloaddeliverywidgeterror".AsWebsiteText());
                return BadRequest(ModelState);
            }

            try
            {
                var cartContext = await HttpContext.GetCartContextAsync();
                if (model.ShipmentWidget != null)
                {
                    model.ShipmentWidget = _deliveryMethodViewModelBuilder.BuildWidget(cartContext, model.SelectedDeliveryMethod.Id);
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when reload delivery widget");
                ModelState.AddModelError("general", "checkout.reloaddeliverywidgeterror".AsWebsiteText());
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates the country to delivery the order.
        /// </summary>
        /// <param name="model">Object containing the selected country.</param>
        [HttpPut]
        [ValidateAntiForgeryToken]
        [Route("setCountry")]
        public async Task<IActionResult> SetCountry(CheckoutViewModel model)
        {
            try
            {
                var cartContext = await HttpContext.GetCartContextAsync();
                //country is changed
                if (cartContext.Cart.Order.CountryCode != model.SelectedCountry)
                {
                    await cartContext.SelectCountryAsync(new SelectCountryArgs
                    {
                        CountryCode = model.SelectedCountry
                    });

                    //Select default delivery method
                    model.DeliveryMethods = _deliveryMethodViewModelBuilder.Build(model.SelectedCountry, cartContext?.Cart.Order?.CurrencyCode);
                    if (model.SelectedDeliveryMethod is null || model.DeliveryMethods.All(x => x.Id != model.SelectedDeliveryMethod.Id))
                    {
                        model.SelectedDeliveryMethod = model.DeliveryMethods?.FirstOrDefault();
                    }
                    var selectShippingOptionArgs = new SelectShippingOptionArgs
                    {
                        ShippingOptionId = model.SelectedDeliveryMethod?.Id
                    };
                    await cartContext.SelectShippingOptionAsync(selectShippingOptionArgs);

                    //Select default payment method
                    model.PaymentMethods = _paymentOptionViewModelBuilder.Build(cartContext);
                    if (model.SelectedPaymentMethod is null || model.PaymentMethods.All(x => x.Id != model.SelectedPaymentMethod.Id))
                    {
                        model.SelectedPaymentMethod = model.PaymentMethods?.FirstOrDefault();
                    }

                    var selectPaymentArgs = new SelectPaymentOptionArgs
                    {
                        PaymentOptionId = model.SelectedPaymentMethod?.Id
                    };
                    await cartContext.SelectPaymentOptionAsync(selectPaymentArgs);
                }

                if (model.PaymentWidget != null)
                {
                    model.PaymentWidget = _paymentOptionViewModelBuilder.BuildWidget(cartContext, model.SelectedPaymentMethod?.Id);
                }
                if (model.ShipmentWidget != null)
                {
                    model.ShipmentWidget = _deliveryMethodViewModelBuilder.BuildWidget(cartContext, model.SelectedDeliveryMethod.Id);
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when changing country to {SelectedCountry}", model.SelectedCountry);
                ModelState.AddModelError("general", "checkout.setcountryerror".AsWebsiteText());
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Submit a discount code to apply for the order.
        /// </summary>
        /// <param name="model">Object containing the discount code.</param>
        [HttpPut]
        [ValidateAntiForgeryToken]
        [Route("setDiscountCode")]
        public async Task<IActionResult> SetDiscountCode(CheckoutViewModel model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.DiscountCode))
                {
                    var cartContext = await HttpContext.GetCartContextAsync();
                    var addDiscountCodeResult = await cartContext.AddDiscountCodeAsync(model.DiscountCode.Trim());
                    if (addDiscountCodeResult)
                    {
                        model.UsedDiscountCodes = cartContext.Cart.DiscountCodes;
                    }

                    await cartContext.CalculateShipmentsAsync();
                    if (model.ShipmentWidget != null)
                    {
                        model.ShipmentWidget = _deliveryMethodViewModelBuilder.BuildWidget(cartContext, model.SelectedDeliveryMethod.Id);
                    }

                    await cartContext.CalculatePaymentsAsync();
                    if (model.PaymentWidget != null)
                    {
                        model.PaymentWidget = _paymentOptionViewModelBuilder.BuildWidget(cartContext, model.SelectedPaymentMethod?.Id);
                    }

                    if (addDiscountCodeResult)
                    {
                        return Ok(model);
                    }
                }

                ModelState.AddModelError(nameof(CheckoutViewModel.DiscountCode), "checkout.discountcodeinvalid".AsWebsiteText());
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when setting discount code {DiscountCode}", model.DiscountCode);
                ModelState.AddModelError("general", "checkout.setdiscountcodeerror".AsWebsiteText());
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Delete applied discount code.
        /// </summary>
        /// <param name="model">Object containing the discount code.</param>
        [HttpDelete]
        [ValidateAntiForgeryToken]
        [Route("deleteDiscountCode")]
        public async Task<IActionResult> DeleteDiscountCode(CheckoutViewModel model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.DiscountCode))
                {
                    var cartContext = await HttpContext.GetCartContextAsync();
                    await cartContext.RemoveDiscountCodeAsync(model.DiscountCode.Trim());
                    model.UsedDiscountCodes = cartContext.Cart.DiscountCodes;
                    await cartContext.CalculateShipmentsAsync();
                    await cartContext.CalculatePaymentsAsync();
                    if (model.ShipmentWidget != null)
                    {
                        model.ShipmentWidget = _deliveryMethodViewModelBuilder.BuildWidget(cartContext, model.SelectedDeliveryMethod.Id);
                    }
                    if (model.PaymentWidget != null)
                    {
                        model.PaymentWidget = _paymentOptionViewModelBuilder.BuildWidget(cartContext, model.SelectedPaymentMethod?.Id);
                    }

                    model.DiscountCode = string.Empty;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when removing discount code {DiscountCode}", model.DiscountCode);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates customer detail information and delivery address on the current shopping cart.
        /// </summary>
        /// <param name="model">Object containing the customer detail information and delivery address.</param>
        [HttpPut]
        [ValidateAntiForgeryToken]
        [Route("setCustomerDetail")]
        public async Task<IActionResult> SetCustomerDetail(CheckoutViewModel model)
        {
            // Validate customer detail
            if (!Validate(ModelState, model))
            {
                return BadRequest(ModelState);
            }

            var cartContext = await HttpContext.GetCartContextAsync();
            //Save customer detail
            try
            {
                await AddOrUpdateCustomerInfoAsync(model, cartContext);
                await AddOrUpdateAdditionalInfoAsync(model, cartContext);
                await AddOrUpdateBillingAddressAsync(model, cartContext);
                await AddOrUpdateDeliveryAddressAsync(model, cartContext);

                var deliveryMethodId = model.SelectedDeliveryMethod?.Id;
                if (!string.IsNullOrEmpty(deliveryMethodId))
                {
                    var selectShippingOptionArgs = new SelectShippingOptionArgs
                    {
                        ShippingOptionId = deliveryMethodId
                    };
                    await cartContext.SelectShippingOptionAsync(selectShippingOptionArgs);
                }

                var paymentMethodId = model.SelectedPaymentMethod?.Id;
                if (!string.IsNullOrEmpty(paymentMethodId))
                {
                    var selectPaymentArgs = new SelectPaymentOptionArgs
                    {
                        PaymentOptionId = paymentMethodId
                    };
                    await cartContext.SelectPaymentOptionAsync(selectPaymentArgs);
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when saving customer detail to cart {Id}", cartContext.Cart.Id);
                ModelState.AddModelError("general", "checkout.generalerror".AsWebsiteText());
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Redeem a gift card for the order.
        /// </summary>
        /// <param name="model">Object containing the gift card.</param>
        [HttpPut]
        [ValidateAntiForgeryToken]
        [Route("redeemGiftCard")]
        public async Task<IActionResult> RedeemGiftCard(CheckoutViewModel model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.GiftCard))
                {
                    var cartContext = await HttpContext.GetCartContextAsync();
                    var addGiftCardResult = await cartContext.AddGiftCardAsync(model.GiftCard.Trim());
                    if (addGiftCardResult)
                    {
                        model.UsedGiftCards = cartContext.Cart.GiftCards;
                    }

                    await cartContext.CalculateShipmentsAsync();
                    if (model.ShipmentWidget != null)
                    {
                        model.ShipmentWidget = _deliveryMethodViewModelBuilder.BuildWidget(cartContext, model.SelectedDeliveryMethod.Id);
                    }

                    await cartContext.CalculatePaymentsAsync();
                    if (model.PaymentWidget != null)
                    {
                        model.PaymentWidget = _paymentOptionViewModelBuilder.BuildWidget(cartContext, model.SelectedPaymentMethod?.Id);
                    }

                    if (addGiftCardResult)
                    {
                        return Ok(model);
                    }
                }

                ModelState.AddModelError(nameof(CheckoutViewModel.GiftCard), "checkout.giftcardinvalid".AsWebsiteText());
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when setting gift card {GiftCard}", model.GiftCard);
                ModelState.AddModelError("general", "checkout.setgiftcarderror".AsWebsiteText());
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates the shipping option of the order.
        /// </summary>
        /// <param name="model">Object containing checkout view model.</param>
        [HttpPut]
        [ValidateAntiForgeryToken]
        [Route("setShippingOption")]
        public async Task<IActionResult> SetShippingOption(CheckoutViewModel model)
        {
            try
            {
                var cartContext = await HttpContext.GetCartContextAsync();
                await cartContext.UpdateShippingSessionAsync(new UpdateShippingSessionArgs() { UpdateSessionData = model.ShippingInfo });
                await cartContext.CalculatePaymentsAsync();

                if (model.PaymentWidget != null)
                {
                    model.PaymentWidget = _paymentOptionViewModelBuilder.BuildWidget(cartContext, model.SelectedPaymentMethod.Id);
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when changing shipping option");
                ModelState.AddModelError("general", "checkout.setshippingoptionerror".AsWebsiteText());
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Abandon applied gift card.
        /// </summary>
        /// <param name="model">Object containing the gift card.</param>
        [HttpDelete]
        [ValidateAntiForgeryToken]
        [Route("abandonGiftCard")]
        public async Task<IActionResult> AbandonGiftCard(CheckoutViewModel model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.GiftCard))
                {
                    var cartContext = await HttpContext.GetCartContextAsync();
                    await cartContext.RemoveGiftCardAsync(model.GiftCard.Trim());
                    model.UsedGiftCards = cartContext.Cart.GiftCards;
                    await cartContext.CalculateShipmentsAsync();
                    await cartContext.CalculatePaymentsAsync();
                    if (model.ShipmentWidget != null)
                    {
                        model.ShipmentWidget = _deliveryMethodViewModelBuilder.BuildWidget(cartContext, model.SelectedDeliveryMethod.Id);
                    }
                    if (model.PaymentWidget != null)
                    {
                        model.PaymentWidget = _paymentOptionViewModelBuilder.BuildWidget(cartContext, model.SelectedPaymentMethod?.Id);
                    }

                    model.GiftCard = string.Empty;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when removing gift card code {GiftCard}", model.GiftCard);
                return BadRequest(ModelState);
            }
        }
        private bool Validate(ModelStateDictionary modelState, CheckoutViewModel viewModel)
        {
            var validationRules = new List<ValidationRuleItem<CheckoutViewModel>>()
            {
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.PhoneNumber)),
                    Rule = model => !string.IsNullOrWhiteSpace(viewModel.CustomerDetails?.PhoneNumber),
                    ErrorMessage = () => "validation.required".AsWebsiteText()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.PhoneNumber)),
                    Rule = model => _userValidationService.IsValidPhone(viewModel.CustomerDetails?.PhoneNumber),
                    ErrorMessage = () => "validation.phone".AsWebsiteText()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.FirstName)),
                    Rule = model => !string.IsNullOrWhiteSpace(model.CustomerDetails?.FirstName),
                    ErrorMessage = () => "validation.required".AsWebsiteText()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.LastName)),
                    Rule = model => !string.IsNullOrEmpty(model.CustomerDetails?.LastName),
                    ErrorMessage = () => "validation.required".AsWebsiteText()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = nameof(viewModel.SelectedDeliveryMethod),
                    Rule = model => model.SelectedDeliveryMethod != null,
                    ErrorMessage = () => "checkout.deliverymethods.required".AsWebsiteText()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.Email)),
                    Rule = model => !string.IsNullOrEmpty(model.CustomerDetails?.Email),
                    ErrorMessage = () => "validation.required".AsWebsiteText()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.Email)),
                    Rule = model => _userValidationService.IsValidEmail(model.CustomerDetails?.Email),
                    ErrorMessage = () => "validation.email".AsWebsiteText()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = nameof(viewModel.SelectedPaymentMethod),
                    Rule = model => model.SelectedPaymentMethod != null,
                    ErrorMessage = () => "checkout.paymentmethods.required".AsWebsiteText()
                }
            };

            if (viewModel.IsBusinessCustomer)
            {
                validationRules.Add(new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = nameof(viewModel.SelectedCompanyAddressId),
                    Rule = model => viewModel.SelectedCompanyAddressId.HasValue && viewModel.SelectedCompanyAddressId.Value != Guid.Empty,
                    ErrorMessage = () => "validation.required".AsWebsiteText()
                });

                return viewModel.IsValid(validationRules, modelState);
            }

            validationRules.AddRange(new List<ValidationRuleItem<CheckoutViewModel>>()
            {
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.Address)),
                    Rule = model => !string.IsNullOrWhiteSpace(model.CustomerDetails?.Address),
                    ErrorMessage = () => "validation.required".AsWebsiteText()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.ZipCode)),
                    Rule = model => !string.IsNullOrWhiteSpace(model.CustomerDetails?.ZipCode),
                    ErrorMessage = () => "validation.required".AsWebsiteText()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.City)),
                    Rule = model => !string.IsNullOrWhiteSpace(model.CustomerDetails?.City),
                    ErrorMessage = () => "validation.required".AsWebsiteText()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.Country)),
                    Rule = model => !string.IsNullOrWhiteSpace(model.CustomerDetails?.Country),
                    ErrorMessage = () => "validation.required".AsWebsiteText()
                },
            });

            if (viewModel.SignUp && !_securityContextService.GetIdentityUserSystemId().HasValue)
            {
                validationRules.Add(new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.Email)),
                    Rule = model =>
                    {
                        var existingUserId = _securityContextService.GetPersonSystemId(viewModel.CustomerDetails.Email);
                        return existingUserId == null || !existingUserId.HasValue;
                    },
                    ErrorMessage = () => "validation.emailinused".AsWebsiteText()
                });
            }

            if (viewModel.ShowAlternativeAddress && !(string.IsNullOrEmpty(viewModel.AlternativeAddress.FirstName) && string.IsNullOrEmpty(viewModel.AlternativeAddress.LastName) && string.IsNullOrEmpty(viewModel.AlternativeAddress.Address)
                && string.IsNullOrEmpty(viewModel.AlternativeAddress.ZipCode) && string.IsNullOrEmpty(viewModel.AlternativeAddress.City) && string.IsNullOrEmpty(viewModel.AlternativeAddress.PhoneNumber)))
            {
                validationRules.AddRange(new List<ValidationRuleItem<CheckoutViewModel>>()
                {

                    new ValidationRuleItem<CheckoutViewModel>
                    {
                        Field = ToModelStateField(nameof(viewModel.AlternativeAddress), nameof(viewModel.AlternativeAddress.PhoneNumber)),
                        Rule = model => !string.IsNullOrWhiteSpace(viewModel.AlternativeAddress?.PhoneNumber),
                        ErrorMessage = () => "validation.required".AsWebsiteText()
                    },
                    new ValidationRuleItem<CheckoutViewModel>
                    {
                        Field = ToModelStateField(nameof(viewModel.AlternativeAddress), nameof(viewModel.AlternativeAddress.PhoneNumber)),
                        Rule = model => _userValidationService.IsValidPhone(viewModel.AlternativeAddress?.PhoneNumber),
                        ErrorMessage = () => "validation.phone".AsWebsiteText()
                    },
                    new ValidationRuleItem<CheckoutViewModel>
                    {
                        Field = ToModelStateField(nameof(viewModel.AlternativeAddress), nameof(viewModel.AlternativeAddress.FirstName)),
                        Rule = model => !string.IsNullOrWhiteSpace(model.AlternativeAddress?.FirstName),
                        ErrorMessage = () => "validation.required".AsWebsiteText()
                    },
                    new ValidationRuleItem<CheckoutViewModel>
                    {
                        Field = ToModelStateField(nameof(viewModel.AlternativeAddress), nameof(viewModel.AlternativeAddress.LastName)),
                        Rule = model => !string.IsNullOrEmpty(model.AlternativeAddress?.LastName),
                        ErrorMessage = () => "validation.required".AsWebsiteText()
                    },
                    new ValidationRuleItem<CheckoutViewModel>
                    {
                        Field = ToModelStateField(nameof(viewModel.AlternativeAddress), nameof(viewModel.AlternativeAddress.Address)),
                        Rule = model => !string.IsNullOrWhiteSpace(model.AlternativeAddress?.Address),
                        ErrorMessage = () => "validation.required".AsWebsiteText()
                    },
                    new ValidationRuleItem<CheckoutViewModel>
                    {
                        Field = ToModelStateField(nameof(viewModel.AlternativeAddress), nameof(viewModel.AlternativeAddress.ZipCode)),
                        Rule = model => !string.IsNullOrWhiteSpace(model.AlternativeAddress?.ZipCode),
                        ErrorMessage = () => "validation.required".AsWebsiteText()
                    },
                    new ValidationRuleItem<CheckoutViewModel>
                    {
                        Field = ToModelStateField(nameof(viewModel.AlternativeAddress), nameof(viewModel.AlternativeAddress.City)),
                        Rule = model => !string.IsNullOrWhiteSpace(model.AlternativeAddress?.City),
                        ErrorMessage = () => "validation.required".AsWebsiteText()
                    },
                    new ValidationRuleItem<CheckoutViewModel>
                    {
                        Field = ToModelStateField(nameof(viewModel.AlternativeAddress), nameof(viewModel.AlternativeAddress.Country)),
                        Rule = model => !string.IsNullOrWhiteSpace(model.AlternativeAddress?.Country),
                        ErrorMessage = () => "validation.required".AsWebsiteText()
                    },
                });
            }

            return viewModel.IsValid(validationRules, modelState);
        }

        private string ToModelStateField(string stateKey, string field)
        {
            return $"{stateKey}-{field.ToCamelCase()}";
        }
        private async Task AddOrUpdateCustomerInfoAsync(CheckoutViewModel model, CartContext cartContext)
        {
            var addOrUpdateCustomerInfoArgs = new AddOrUpdateCustomerInfoArgs
            {
                CustomerInfo = new CustomerInfo
                {
                    CustomerNumber = cartContext.Cart.Order.CustomerInfo?.CustomerNumber,
                    CustomerType = cartContext.Cart.Order.CustomerInfo?.CustomerType ?? CustomerType.Person,
                    PersonSystemId = cartContext.PersonSystemId,
                    OrganizationSystemId = cartContext.OrganizationSystemId,
                    FirstName = model.CustomerDetails.FirstName,
                    LastName = model.CustomerDetails.LastName,
                    Email = model.CustomerDetails.Email,
                    Phone = model.CustomerDetails.PhoneNumber
                }
            };

            if (cartContext.PersonSystemId != null && cartContext.PersonSystemId != Guid.Empty)
            {
                var person = _personService.Get(cartContext.PersonSystemId.Value);
                if (person != null)
                {
                    //Set customer Social Security Number.
                    var ssn = person.Fields.GetValue<string>("SocialSecurityNumber");
                    addOrUpdateCustomerInfoArgs.CustomerInfo.NationalIdentificationNumber = ssn;
                }
            }

            await cartContext.AddOrUpdateCustomerInfoAsync(addOrUpdateCustomerInfoArgs);
        }
        private async Task AddOrUpdateBillingAddressAsync(CheckoutViewModel model, CartContext cartContext)
        {
            var billingAddress = new Sales.Address();
            if (model.IsBusinessCustomer && model.SelectedCompanyAddressId.HasValue)
            {
                var organization = _organizationService.Get(cartContext.OrganizationSystemId.Value);
                billingAddress.MapFrom(organization.Addresses.Single(a => a != null && a.SystemId == model.SelectedCompanyAddressId.Value).MapTo<AddressViewModel>());
                billingAddress.MobilePhone = model.CustomerDetails.PhoneNumber;
                billingAddress.FirstName = model.CustomerDetails.FirstName;
                billingAddress.LastName = model.CustomerDetails.LastName;
                billingAddress.Email = model.CustomerDetails.Email;
                billingAddress.OrganizationName = model.CompanyName;
            }
            else
            {
                billingAddress.MapFrom(model.CustomerDetails);
            }

            await cartContext.AddOrUpdateBillingAddressAsync(billingAddress);
        }

        private async Task AddOrUpdateDeliveryAddressAsync(CheckoutViewModel model, CartContext cartContext)
        {
            var deliveryAddress = new Sales.Address();
            if (model.IsBusinessCustomer)
            {
                if (model.SelectedCompanyAddressId.HasValue)
                {
                    var organization = _organizationService.Get(cartContext.OrganizationSystemId.Value);
                    deliveryAddress.MapFrom(organization.Addresses.Single(a => a != null && a.SystemId == model.SelectedCompanyAddressId.Value).MapTo<AddressViewModel>());
                    deliveryAddress.MobilePhone = model.CustomerDetails.PhoneNumber;
                    deliveryAddress.FirstName = model.CustomerDetails.FirstName;
                    deliveryAddress.LastName = model.CustomerDetails.LastName;
                    deliveryAddress.Email = model.CustomerDetails.Email;
                    deliveryAddress.OrganizationName = model.CompanyName;

                    await cartContext.AddOrUpdateDeliveryAddressAsync(deliveryAddress);
                }
            }
            else
            {
                if (model.ShowAlternativeAddress)
                {
                    deliveryAddress.MapFrom(model.AlternativeAddress);
                }
                else
                {
                    deliveryAddress.MapFrom(model.CustomerDetails);
                }

                await cartContext.AddOrUpdateDeliveryAddressAsync(deliveryAddress);
            }
        }

        private async Task AddOrUpdateAdditionalInfoAsync(CheckoutViewModel model, CartContext cartContext)
        {
            await cartContext.AddOrUpdateAdditionalInfoAsync(new { SignUp = model.SignUp ? (bool?)true : null });
        }

        private async Task UpdateOrderCommentAsync(CheckoutViewModel model, CartContext cartContext)
        {
            var updateOrderCommentArgs = new AddOrUpdateOrderCommentArgs
            {
                Comments = model.OrderNote
            };
            await cartContext.UpdateOrderCommentAsync(updateOrderCommentArgs);
        }

        private void UpdatePersonInformation(CheckoutViewModel model, CartContext cartContext, Person person)
        {
            var isUpdated = false;
            var customerInfo = cartContext.Cart.Order.CustomerInfo;
            var billingAddress = cartContext.Cart.Order.BillingAddress;
            var shippingAddress = cartContext.Cart.Order.ShippingInfo.FirstOrDefault()?.ShippingAddress;

            if (customerInfo != null)
            {
                var firstName = person.FirstName ?? string.Empty;
                var lastName = person.LastName ?? string.Empty;
                var email = person.Email ?? string.Empty;
                var phone = person.Phone ?? string.Empty;

                //Update personal information
                if (!firstName.Equals(customerInfo.FirstName, StringComparison.OrdinalIgnoreCase)
                    || !lastName.Equals(customerInfo.LastName, StringComparison.OrdinalIgnoreCase)
                    || !email.Equals(customerInfo.Email, StringComparison.OrdinalIgnoreCase)
                    || !phone.Equals(customerInfo.Phone, StringComparison.OrdinalIgnoreCase))
                {
                    person.FirstName = customerInfo.FirstName;
                    person.LastName = customerInfo.LastName;
                    person.Email = customerInfo.Email;
                    person.Phone = customerInfo.Phone;
                    isUpdated = true;
                }
            }

            if (!model.IsBusinessCustomer)
            {
                if (billingAddress != null)
                {
                    var addressType = _addressTypeService.Get(AddressTypeNameConstants.Address);
                    var address = person.Addresses.FirstOrDefault(x => x.AddressTypeSystemId == addressType.SystemId);
                    if (address == null)
                    {
                        address = new Customers.Address(addressType.SystemId);
                        person.Addresses.Add(address);
                    }

                    //Update billing address
                    if (!IsEquals(address, billingAddress))
                    {
                        address.Address1 = billingAddress.Address1;
                        address.Address2 = billingAddress.Address2;
                        address.CareOf = billingAddress.CareOf;
                        address.ZipCode = billingAddress.ZipCode;
                        address.City = billingAddress.City;
                        address.Country = billingAddress.Country;
                        address.PhoneNumber = billingAddress.MobilePhone;
                        isUpdated = true;
                    }
                }

                if (shippingAddress != null)
                {
                    var alternativeAddressType = _addressTypeService.Get(AddressTypeNameConstants.AlternativeAddress);
                    var alternativeAddress = person.Addresses.FirstOrDefault(x => x.AddressTypeSystemId == alternativeAddressType.SystemId);
                    if (alternativeAddress == null)
                    {
                        alternativeAddress = new Customers.Address(alternativeAddressType.SystemId);
                        person.Addresses.Add(alternativeAddress);
                    }

                    //Update shipping address
                    if (!IsEquals(alternativeAddress, shippingAddress) && model.ShowAlternativeAddress)
                    {
                        alternativeAddress.Address1 = shippingAddress.Address1;
                        alternativeAddress.Address2 = shippingAddress.Address2;
                        alternativeAddress.CareOf = shippingAddress.CareOf;
                        alternativeAddress.ZipCode = shippingAddress.ZipCode;
                        alternativeAddress.City = shippingAddress.City;
                        alternativeAddress.Country = shippingAddress.Country;
                        alternativeAddress.PhoneNumber = shippingAddress.MobilePhone;
                        isUpdated = true;
                    }
                }
            }

            if (isUpdated)
            {
                using (_securityContextService.ActAsSystem())
                {
                    _personService.Update(person);
                }
            }
        }

        private bool IsEquals(Customers.Address address1, Sales.Address address2)
        {
            if (address2 == null || address1 == null)
            {
                return false;
            }
            return string.Equals(address1.Address1 ?? string.Empty, address2.Address1 ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(address1.Address2 ?? string.Empty, address2.Address2 ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(address1.CareOf ?? string.Empty, address2.CareOf ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(address1.City ?? string.Empty, address2.City ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(address1.State ?? string.Empty, address2.State ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(address1.Country ?? string.Empty, address2.Country ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(address1.ZipCode ?? string.Empty, address2.ZipCode ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(address1.PhoneNumber ?? string.Empty, address2.MobilePhone ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        private async Task RegisterNewUserAsync(CheckoutViewModel model, CartContext cartContext)
        {
            var customerInfo = cartContext.Cart.Order.CustomerInfo;
            var billingAddress = cartContext.Cart.Order.BillingAddress;
            var shippingAddress = cartContext.Cart.Order.ShippingInfo.FirstOrDefault()?.ShippingAddress;

            var template = _templateService.Get<PersonFieldTemplate>(typeof(CustomerArea), CustomerTemplateIdConstants.B2CPersonTemplate);
            var person = new Person(template.SystemId)
            {
                FirstName = customerInfo.FirstName,
                LastName = customerInfo.LastName,
                Email = customerInfo.Email,
                LoginCredential = { Username = customerInfo.Email, NewPassword = _loginService.GeneratePassword() },
                Phone = customerInfo.Phone
            };

            if (billingAddress != null)
            {
                var addressType = _addressTypeService.Get(AddressTypeNameConstants.Address);
                var address = billingAddress.MapTo<Customers.Address>();
                address.AddressTypeSystemId = addressType.SystemId;
                person.Addresses.Add(address);
            }

            if (shippingAddress != null)
            {
                var alternativeAddressType = _addressTypeService.Get(AddressTypeNameConstants.AlternativeAddress);
                var address = shippingAddress.MapTo<Customers.Address>();
                address.AddressTypeSystemId = alternativeAddressType.SystemId;
                person.Addresses.Add(address);
            }

            using (_securityContextService.ActAsSystem())
            {
                _personService.Create(person);
            }

            _mailService.SendEmail(_welcomeEmailDefinitionResolver.Get(person.MapTo<WelcomeEmailModel>(), person.Email), false);

            if (_loginService.Login(person.LoginCredential.Username, person.LoginCredential.NewPassword))
            {
                var changeCustomerArgs = new ChangeCustomerArgs
                {
                    CustomerNumber = person.Id,
                    CustomerType = CustomerType.Person,
                    PersonSystemId = person.SystemId,
                    OrganizationSystemId = null
                };
                await cartContext.ChangeCustomerAsync(changeCustomerArgs);
            }
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
