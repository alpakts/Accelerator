using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Utilities;
using Litium.Accelerator.ViewModels.Checkout;
using Litium.Accelerator.ViewModels.Persons;
using Litium.Customers;
using Litium.Globalization;
using Litium.Runtime.AutoMapper;
using Litium.Sales;
using Litium.Security;
using Litium.Web.Routing;

namespace Litium.Accelerator.Builders.Checkout
{
    public class CheckoutViewModelBuilder : IViewModelBuilder<CheckoutViewModel>
    {
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly RouteRequestLookupInfoAccessor _routeRequestLookupInfoAccessor;
        private readonly SecurityContextService _securityContextService;
        private readonly DeliveryMethodViewModelBuilder _deliveryMethodViewModelBuilder;
        private readonly PaymentOptionViewModelBuilder _paymentOptionViewModelBuilder;
        private readonly PersonService _personService;
        private readonly AddressTypeService _addressTypeService;
        private readonly PersonStorage _personStorage;
        private readonly ISignInUrlResolver _signInUrlResolver;
        private readonly CountryService _countryService;
        private readonly PaymentService _paymentService;
        private readonly CurrencyService _currencyService;
        private readonly CartContextAccessor _cartContextAccessor;
        public CheckoutViewModelBuilder(
            RequestModelAccessor requestModelAccessor,
            RouteRequestLookupInfoAccessor routeRequestLookupInfoAccessor,
            SecurityContextService securityContextService,
            DeliveryMethodViewModelBuilder deliveryMethodViewModelBuilder,
            PaymentOptionViewModelBuilder paymentOptionViewModelBuilder,
            PersonService personService,
            ISignInUrlResolver signInUrlResolver,
            AddressTypeService addressTypeService,
            CountryService countryService,
            PersonStorage personStorage,
            PaymentService paymentService,
            CurrencyService currencyService,
            CartContextAccessor cartContextAccessor
            )
        {
            _requestModelAccessor = requestModelAccessor;
            _routeRequestLookupInfoAccessor = routeRequestLookupInfoAccessor;
            _securityContextService = securityContextService;
            _deliveryMethodViewModelBuilder = deliveryMethodViewModelBuilder;
            _paymentOptionViewModelBuilder = paymentOptionViewModelBuilder;
            _personService = personService;
            _addressTypeService = addressTypeService;
            _countryService = countryService;
            _personStorage = personStorage;
            _signInUrlResolver = signInUrlResolver;
            _paymentService = paymentService;
            _currencyService = currencyService;
            _cartContextAccessor = cartContextAccessor;
        }

        public virtual async Task<CheckoutViewModel> BuildAsync(CartContext cartContext)
        {
            var requestModel = _requestModelAccessor.RequestModel;
            var orderDetails = cartContext?.Cart.Order;
            var countryCode = orderDetails?.CountryCode ?? requestModel.CountryModel.Country.Id;
            var currencyCode = orderDetails?.CurrencyCode ?? _currencyService.Get(requestModel.CountryModel.Country.CurrencySystemId)?.Id;

            var deliveryMethods = _deliveryMethodViewModelBuilder.Build(countryCode, currencyCode);
            var paymentOptions = _paymentOptionViewModelBuilder.Build(cartContext);
            _signInUrlResolver.TryGet(_routeRequestLookupInfoAccessor.RouteRequestLookupInfo, out var loginPageUrl);
            var checkoutModeInt = requestModel.WebsiteModel.GetValue<int>(AcceleratorWebsiteFieldNameConstants.CheckoutMode);
            var checkoutMode = checkoutModeInt == 0 ? CheckoutMode.Both : (CheckoutMode)checkoutModeInt;
            var model = new CheckoutViewModel()
            {
                Authenticated = _securityContextService.GetIdentityUserSystemId().HasValue,
                CheckoutMode = (int)checkoutMode,
                DeliveryMethods = deliveryMethods,
                PaymentMethods = paymentOptions,
                IsBusinessCustomer = _personStorage.CurrentSelectedOrganization != null,
                CompanyName = _personStorage.CurrentSelectedOrganization?.Name,
                CheckoutUrl = cartContext?.CheckoutFlowInfo.CheckoutPageUrl,
                TermsUrl = cartContext?.CheckoutFlowInfo.TermsUrl,
                LoginUrl = loginPageUrl,
                UsedDiscountCodes = cartContext?.Cart.DiscountCodes,
                UsedGiftCards = cartContext?.Cart.GiftCards
            };

            model.SignUp = cartContext?.Cart.Order.AdditionalInfo?.TryGetValue(nameof(CheckoutViewModel.SignUp), out _) == true;

            await SetSelectedShippingOptionAsync(model, orderDetails, cartContext, deliveryMethods);
            GetSelectedPaymentOptionId(model, orderDetails);
            GetCustomerDetails(model, cartContext);

            //Connected country to the channel must be selected
            if (model.IsBusinessCustomer)
            {
                var countries = _countryService.GetAll().Where(x => _requestModelAccessor.RequestModel.ChannelModel.Channel.CountryLinks.Any(y => y.CountrySystemId == x.SystemId));
                var companyAddresses = _personStorage.CurrentSelectedOrganization?.Addresses?.Where(x => countries.Any(y => y.Id == x.Country))
                    .Select(address => address.MapTo<AddressViewModel>())?.Where(address => address != null)?.ToList();
                model.CompanyAddresses = companyAddresses;
                model.SelectedCompanyAddressId = companyAddresses?.FirstOrDefault(x => x.Country == countryCode)?.SystemId;
            }
            else
            {
                model.CustomerDetails.Country = countryCode;
            }
            await EnsureTagAsync();
            return model;
        }

        private void GetCustomerDetails(CheckoutViewModel model, CartContext cartContext)
        {
            model.CustomerDetails = new CustomerDetailsViewModel();


            if (cartContext.Cart != null && !cartContext.Cart.Order.BillingAddress.IsEmpty())
            {
                model.CustomerDetails.MapFrom(cartContext.Cart.Order.BillingAddress);

                var shippingAddress = cartContext.Cart.Order.ShippingInfo.FirstOrDefault()?.ShippingAddress;
                if (!shippingAddress.IsEmpty())
                {
                    if (model.AlternativeAddress == null)
                    {
                        model.AlternativeAddress = new CustomerDetailsViewModel();
                    }
                    model.AlternativeAddress.MapFrom(shippingAddress);
                }
            }
            else
            {
                var personSystemId = _securityContextService.GetIdentityUserSystemId();
                if (!personSystemId.HasValue)
                {
                    return;
                }

                var person = _personService.Get(personSystemId.Value);
                if (person == null)
                {
                    return;
                }

                if (model.IsBusinessCustomer)
                {
                    //Use person phone for company customer
                    model.CustomerDetails.PhoneNumber = person.Phone;
                }
                else
                {
                    var addressType = _addressTypeService.Get(AddressTypeNameConstants.Address);
                    var address = person.Addresses.FirstOrDefault(x => x.AddressTypeSystemId == addressType.SystemId);
                    if (address != null)
                    {
                        model.CustomerDetails.MapFrom(address);
                    }

                    addressType = _addressTypeService.Get(AddressTypeNameConstants.AlternativeAddress);
                    address = person.Addresses.FirstOrDefault(x => x.AddressTypeSystemId == addressType.SystemId);
                    if (address != null)
                    {
                        model.AlternativeAddress = new CustomerDetailsViewModel();
                        model.AlternativeAddress.MapFrom(address);
                    }
                }

                model.CustomerDetails.FirstName = person.FirstName;
                model.CustomerDetails.LastName = person.LastName;
                model.CustomerDetails.Email = person.Email;
            }
        }

        private async Task SetSelectedShippingOptionAsync(CheckoutViewModel model, SalesOrder orderDetails, CartContext cartContext, List<DeliveryMethodViewModel> deliveryMethods)
        {
            var deliveryMethodPaymentCheckout = deliveryMethods.FirstOrDefault(x => x.IntegrationType == ShippingIntegrationType.PaymentCheckout);
            if (deliveryMethodPaymentCheckout == null)
            {
                var shippingInfo = orderDetails?.ShippingInfo.FirstOrDefault();
                if (shippingInfo != null)
                {
                    model.SelectedDeliveryMethod = model.DeliveryMethods.FirstOrDefault(x => x.Id == shippingInfo.ShippingOption);
                    model.DefaultDeliveryMethod = null;
                }
                else
                {
                    model.SelectedDeliveryMethod =  model.DeliveryMethods.FirstOrDefault();
                    model.DefaultDeliveryMethod = model.SelectedDeliveryMethod?.Id;
                }

                if(model.SelectedDeliveryMethod.IntegrationType == ShippingIntegrationType.DeliveryOptions)
                {
                    //select the first shipping option. 
                    var firstDeliveryOptionGroup = cartContext.ShippingFlowResults.FirstOrDefault()?.DeliveryOptionGroups.FirstOrDefault();
                    if (firstDeliveryOptionGroup?.DeliveryOptions?.Count == 1)
                    {
                        await cartContext.UpdateShippingSessionAsync(new UpdateShippingSessionArgs() { UpdateSessionData = firstDeliveryOptionGroup?.DeliveryOptions?.FirstOrDefault().Id });
                    }
                }
            }
            else
            {
                model.SelectedDeliveryMethod = deliveryMethodPaymentCheckout;
                model.DefaultDeliveryMethod = deliveryMethodPaymentCheckout.Id;
                await cartContext.SelectShippingOptionAsync(new SelectShippingOptionArgs
                {
                    ShippingOptionId = deliveryMethodPaymentCheckout.Id
                });
            }
        }

        private void GetSelectedPaymentOptionId(CheckoutViewModel model, SalesOrder orderDetails)
        {
            var paymentSystemId = orderDetails?.OrderPaymentLinks.FirstOrDefault()?.PaymentSystemId;
            var payment = _paymentService.Get(paymentSystemId.GetValueOrDefault());
            if (payment != null)
            {
                model.SelectedPaymentMethod = model.PaymentMethods.FirstOrDefault(x => x.Id.ProviderId == payment.PaymentOption.ProviderId && x.Id.OptionId == payment.PaymentOption.OptionId);
                model.DefaultPaymentMethod = null;
            }
            else
            {
                model.SelectedPaymentMethod = model.PaymentMethods.FirstOrDefault();
                model.DefaultPaymentMethod = model.SelectedPaymentMethod?.Id;
            }
        }

        private async Task EnsureTagAsync()
        {
            var tagsClone = _cartContextAccessor.CartContext.Cart.Tags.ToHashSet();
            tagsClone.Remove(OrderTaggingConstants.AwaitOrderApproval);
            if (_personStorage.HasPlacerRole && !_personStorage.HasApproverRole)
            {
                tagsClone.Add(OrderTaggingConstants.AwaitOrderApproval);
            }
            await _cartContextAccessor.CartContext.SetTagsAsync(tagsClone);
        }

    }
}
