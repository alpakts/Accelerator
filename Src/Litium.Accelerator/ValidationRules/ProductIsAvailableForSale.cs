using System;
using System.Linq;
using Litium.Globalization;
using Litium.Sales;
using Litium.Sales.Factory;
using Litium.Security;
using Litium.Validations;
using Litium.Websites;

namespace Litium.Accelerator.ValidationRules
{
    /// <summary>
    /// Validates whether product is still available to buy, since it was last put to cart.
    /// </summary>
    public class ProductIsAvailableForSale : ValidationRuleBase<ValidateCartContextArgs>
    {
        private readonly ISalesOrderRowFactory _salesOrderRowFactory;
        private readonly SecurityContextService _securityContextService;
        private readonly CountryService _countryService;
        private readonly CurrencyService _currencyService;
        private readonly WebsiteService _websiteService;
        private readonly ChannelService _channelService;
        private readonly LanguageService _languageService;

        public ProductIsAvailableForSale(
            ISalesOrderRowFactory salesOrderRowFactory,
            SecurityContextService securityContextService,
            CountryService countryService,
            CurrencyService currencyService,
            WebsiteService websiteService,
            ChannelService channelService,
            LanguageService languageService)
        {
            _salesOrderRowFactory = salesOrderRowFactory;
            _securityContextService = securityContextService;
            _countryService = countryService;
            _currencyService = currencyService;
            _websiteService = websiteService;
            _channelService = channelService;
            _languageService = languageService;
        }

        public override ValidationResult Validate(ValidateCartContextArgs entity, ValidationMode validationMode)
        {
            var result = new ValidationResult();
            var order = entity.Cart.Order;

            if (order.Rows.Count > 0)
            {
                var personId = order.CustomerInfo?.PersonSystemId ?? _securityContextService.GetIdentityUserSystemId() ?? Guid.Empty;
                var orderRows = order.Rows.Where(x => x.OrderRowType == OrderRowType.Product)
                                          .Select(orderRow => _salesOrderRowFactory.Create(new CreateSalesOrderRowArgs
                                          {
                                              ArticleNumber = orderRow.ArticleNumber,
                                              Quantity = orderRow.Quantity,
                                              ChannelSystemId = order.ChannelSystemId ?? Guid.Empty,
                                              CountrySystemId = _countryService.Get(order.CountryCode)?.SystemId ?? Guid.Empty,
                                              CurrencySystemId = _currencyService.Get(order.CurrencyCode)?.SystemId ?? Guid.Empty
                                          }));

                if (orderRows.Any(result => result is null))
                {
                    var channel = _channelService.Get(order.ChannelSystemId.GetValueOrDefault());
                    var website = channel is null
                        ? null
                        : _websiteService.Get(channel.WebsiteSystemId.GetValueOrDefault());

                    var culture = _languageService.Get(channel?.WebsiteLanguageSystemId.GetValueOrDefault() ?? Guid.Empty)?.CultureInfo
                        ?? _languageService.GetDefault().CultureInfo;

                    var formattableText = (website is null
                           ? null
                           : website.Texts["sales.validation.product.nolongeravailableforsale", culture])
                           ?? "Some products are no longer available for sale, since last time the cart was re-calculated. Please check your shopping cart before placing the order.";

                    result.AddError("Cart", formattableText);
                }
            }

            return result;
        }
    }
}
