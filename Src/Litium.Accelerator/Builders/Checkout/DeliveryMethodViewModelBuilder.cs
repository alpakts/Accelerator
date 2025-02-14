using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Shipments;
using Litium.Accelerator.ViewModels.Checkout;
using Litium.Globalization;
using Litium.Sales;

namespace Litium.Accelerator.Builders.Checkout
{
    public class DeliveryMethodViewModelBuilder : IViewModelBuilder<DeliveryMethodViewModel>
    {
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly CurrencyService _currencyService;
        private readonly CountryService _countryService;
        private readonly ChannelService _channelService;
        private readonly ShippingProviderService _shippingProviderService;

        public DeliveryMethodViewModelBuilder(
            RequestModelAccessor requestModelAccessor,
            CurrencyService currencyService,
            CountryService countryService,
            ChannelService channelService,
            ShippingProviderService shippingProviderService)
        {
            _requestModelAccessor = requestModelAccessor;
            _currencyService = currencyService;
            _countryService = countryService;
            _channelService = channelService;
            _shippingProviderService = shippingProviderService;
        }

        public virtual List<DeliveryMethodViewModel> Build(string countryCode, string currencyCode)
        {
            var currency = _currencyService.Get(currencyCode);
            if (currency == null)
            {
                return new List<DeliveryMethodViewModel>();
            }
            var countrySystemId = _countryService.Get(countryCode)?.SystemId;
            var channelSystemId = _requestModelAccessor.RequestModel.ChannelModel?.Channel?.SystemId;
              
            List<DeliveryMethodViewModel> deliveryMethods = new List<DeliveryMethodViewModel>();
            var channel = _channelService.Get(channelSystemId.HasValue ? channelSystemId.Value : Guid.Empty);

            foreach (var shippingOption in channel?.CountryLinks?.Where(x => x.CountrySystemId == countrySystemId)?.SelectMany(x => x.ShippingOptions))
            {
                //Skip shipping option if provider or option don't exist in the system
                var shippingProviderOption = _shippingProviderService.Get(shippingOption.Id.ProviderId)?.Options.FirstOrDefault(x => x.Id == shippingOption.Id.OptionId);
                if (shippingProviderOption != null)
                {
                    var shippingProvider = shippingOption.Id;
                    deliveryMethods.Add(new DeliveryMethodViewModel
                    {
                        Id = shippingProvider,
                        FormattedPrice = currency.Format(shippingOption.Fee ?? 0, true, CultureInfo.CurrentCulture),
                        Name = string.IsNullOrWhiteSpace(shippingOption.Name) ? shippingProvider.ToString() : shippingOption.Name,
                        Price = shippingOption.Fee,
                        IntegrationType = shippingProviderOption.IntegrationType
                    });
                }
            }

            return deliveryMethods;
        }

        public virtual ShipmentWidgetResult BuildWidget(CartContext cartContext, ProviderOptionIdentifier shippingOptionIdentifier)
        {
            if (cartContext == null || !cartContext.ShippingFlowResults.Any() || !cartContext.Cart.Order.ShippingInfo.Any())
            {
                return null;
            }

            var shippingInfo = cartContext.Cart.Order.ShippingInfo.FirstOrDefault();
            if (shippingInfo.ShippingOption.OptionId == shippingOptionIdentifier.OptionId && shippingInfo.ShippingOption.ProviderId == shippingOptionIdentifier.ProviderId)
            {
                var shippingFlowResult = cartContext.ShippingFlowResults.SingleOrDefault(x => x.Success);
                if (shippingFlowResult != null && !string.IsNullOrEmpty(shippingFlowResult.HtmlSnippet))
                {
                    return new ShipmentWidgetResult
                    {
                        Id = shippingOptionIdentifier,
                        ResponseString = shippingFlowResult.HtmlSnippet
                    };
                }
            }
           
            return null;
        }
    }
}
