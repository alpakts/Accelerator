using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Litium.Globalization;
using Litium.Products;
using Litium.Products.StockStatusCalculator;
using Litium.Sales;
using Litium.Security;
using Litium.Validations;
using Litium.Web;
using Litium.Websites;

namespace Litium.Accelerator.ValidationRules
{
    /// <summary>
    /// Validates whether product is still in stock.
    /// </summary>
    public class ProductsAreInStock : ValidationRuleBase<ValidateCartContextArgs>
    {
        private readonly IStockStatusCalculator _stockStatusCalculator;
        private readonly SecurityContextService _securityContextService;
        private readonly CountryService _countryService;
        private readonly VariantService _variantService;
        private readonly WebsiteService _websiteService;
        private readonly ChannelService _channelService;
        private readonly LanguageService _languageService;

        public ProductsAreInStock(
            IStockStatusCalculator stockStatusCalculator,
            SecurityContextService securityContextService,
            CountryService countryService,
            VariantService variantService,
            WebsiteService websiteService,
            ChannelService channelService,
            LanguageService languageService)
        {
            _stockStatusCalculator = stockStatusCalculator;
            _securityContextService = securityContextService;
            _countryService = countryService;
            _variantService = variantService;
            _websiteService = websiteService;
            _channelService = channelService;
            _languageService = languageService;
        }

        public override ValidationResult Validate(ValidateCartContextArgs entity, ValidationMode validationMode)
        {
            throw new NotSupportedException("Validation need to be done async.");
        }

        public override Task<ValidationResult> ValidateAsync(ValidateCartContextArgs entity, ValidationMode validationMode)
        {
            var result = new PostActionValidationResult();
            var order = entity.Cart.Order;

            if (order.Rows.Count > 0)
            {
                var personId = order.CustomerInfo?.PersonSystemId ?? _securityContextService.GetIdentityUserSystemId() ?? Guid.Empty;
                var countryId = _countryService.Get(order.CountryCode)?.SystemId;

                var outOfStocksProducts = new List<string>();
                var notEnoughInStocksProducts = new List<string>();
                foreach (var row in order.Rows.Where(x => x.OrderRowType == OrderRowType.Product))
                {
                    var variant = _variantService.Get(row.ArticleNumber);
                    if (variant is not null)
                    {
                        _stockStatusCalculator.GetStockStatuses(new StockStatusCalculatorArgs
                        {
                            UserSystemId = personId,
                            CountrySystemId = countryId
                        }, new StockStatusCalculatorItemArgs
                        {
                            VariantSystemId = variant.SystemId,
                            Quantity = row.Quantity
                        }).TryGetValue(variant.SystemId, out var stockStatus);

                        var existingStocks = stockStatus?.InStockQuantity.GetValueOrDefault();
                        //If stock status is not returned or the actual stock level is zero or below.
                        if (stockStatus is null || existingStocks <= decimal.Zero)
                        {
                            //Remove the order row from the shopping cart.
                            var updateItem = new AddOrUpdateCartItemArgs
                            {
                                ArticleNumber = row.ArticleNumber,
                                Quantity = 0,
                                ConstantQuantity = true,
                            };
                            result.Actions.Add(ctx => ctx.AddOrUpdateItemAsync(updateItem));
                            outOfStocksProducts.Add(row.Description ?? row.ArticleNumber);
                        }
                        else if (row.Quantity > existingStocks)
                        {
                            //Update the order row with available amount in stock.
                            var updateItem = new AddOrUpdateCartItemArgs
                            {
                                ArticleNumber = row.ArticleNumber,
                                Quantity = existingStocks.Value,
                                ConstantQuantity = true,
                            };
                            result.Actions.Add(ctx => ctx.AddOrUpdateItemAsync(updateItem));
                            notEnoughInStocksProducts.Add(row.Description ?? row.ArticleNumber);
                        }
                    }
                }

                if (result.Actions.Count > 0)
                {
                    result.Actions.Add(ctx => ctx.CalculatePaymentsAsync());

                    var channel = _channelService.Get(order.ChannelSystemId.GetValueOrDefault());
                    var website = channel is null
                        ? null
                        : _websiteService.Get(channel.WebsiteSystemId.GetValueOrDefault());

                    var culture = _languageService.Get(channel?.WebsiteLanguageSystemId.GetValueOrDefault() ?? Guid.Empty)?.CultureInfo
                        ?? _languageService.GetDefault().CultureInfo;

                    var sb = new StringBuilder();
                    if (outOfStocksProducts.Count > 0)
                    {
                        var formattableText = (website is null
                            ? null
                            : website.Texts["sales.validation.product.outofstock", culture])
                            ?? "{0} is out of stock. The product has been removed from your shopping cart.";

                        outOfStocksProducts.ForEach(x => sb.AppendFormat(formattableText, x));
                    }
                    if (notEnoughInStocksProducts.Count > 0)
                    {
                        var formattableText = (website is null
                            ? null
                            :  website.Texts["sales.validation.product.notenoughinstock", culture])
                            ?? "There are not enough {0} in stock. The shopping cart has been updated with the amount that we can deliver.";

                        notEnoughInStocksProducts.ForEach(x => sb.AppendFormat(formattableText, x));
                    }
                    result.AddError("Cart", sb.ToString());
                }
            }

            return Task.FromResult<ValidationResult>(result);
        }
    }
}
