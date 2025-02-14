using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Litium.Accelerator.Builders.Order;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Routing;
using Litium.Accelerator.ViewModels.Framework;
using Litium.ComponentModel;
using Litium.FieldFramework;
using Litium.FieldFramework.FieldTypes;
using Litium.Globalization;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Sales;
using Litium.Sales.DiscountTypes;
using Litium.Web;
using Litium.Web.Models;
using Litium.Web.Models.Products;

namespace Litium.Accelerator.Builders.Framework
{
    public class CartViewModelBuilder : IViewModelBuilder<CartViewModel>
    {
        private readonly HashSet<string> _discountTypes = new()
        {
            nameof(DiscountedProductPrice),
            nameof(ProductDiscount),
            nameof(BuyXGetCheapestDiscount),
            nameof(MixAndMatch)
        };

        private record ProductDiscountMap(decimal ExcludingVat, decimal IncludingVat);

        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly UrlService _urlService;
        private readonly ChannelService _channelService;
        private readonly CurrencyService _currencyService;
        private readonly BaseProductService _baseProductService;
        private readonly VariantService _variantService;
        private readonly UnitOfMeasurementService _unitOfMeasurementService;

        public CartViewModelBuilder(
            RequestModelAccessor requestModelAccessor,
            UrlService urlService,
            ChannelService channelService,
            CurrencyService currencyService,
            BaseProductService baseProductService,
            VariantService variantService,
            UnitOfMeasurementService unitOfMeasurementService)
        {
            _requestModelAccessor = requestModelAccessor;
            _urlService = urlService;
            _channelService = channelService;
            _currencyService = currencyService;
            _baseProductService = baseProductService;
            _variantService = variantService;
            _unitOfMeasurementService = unitOfMeasurementService;
        }

        public CartViewModel Build(CartContext cartContext)
        {
            if (cartContext is null)
            {
                return new CartViewModel
                {
                    Quantity = "0",
                };
            }

            var includeVat = IncludeVat(cartContext.Cart);
            decimal discount = cartContext.Cart.Order.GetNonProductDiscountTotal(includeVat);
            var deliveryCost = includeVat ? cartContext.Cart.Order.ShippingCostIncludingVat : cartContext.Cart.Order.ShippingCostExcludingVat;
            var paymentCost = includeVat ? cartContext.Cart.Order.TotalFeesIncludingVat : cartContext.Cart.Order.TotalFeesExcludingVat;
            var quantity = cartContext.Cart.Order.Rows.Where(x => x.OrderRowType == OrderRowType.Product).Sum(x => x.Quantity);
            var currency = _currencyService.Get(cartContext.CurrencyCode);
            var checkoutPage = _requestModelAccessor.RequestModel.WebsiteModel.GetValue<PointerPageItem>(AcceleratorWebsiteFieldNameConstants.CheckoutPage).MapTo<LinkModel>();
            var productDiscountMap = BuildProductDiscountMap(cartContext.Cart.Order);
            var orderRows = cartContext.Cart.Order.Rows.Where(x => x.OrderRowType == OrderRowType.Product).Select(row => BuildOrderRow(row, currency, includeVat, productDiscountMap)).ToList();
            var discountRows = BuildDiscountRows(cartContext.Cart.Order, _discountTypes, includeVat, currency);
            var orderRowsWithoutFreeGift = orderRows.Where(x => !x.IsFreeGift);
            var orderTotal = includeVat ? orderRowsWithoutFreeGift.Sum(x => x.TotalPriceIncludingVat)
                : orderRowsWithoutFreeGift.Sum(x => x.TotalPriceExcludingVat);
            if (discountRows.Count > 0)
            {
                orderTotal += discountRows.Sum(x => includeVat ? x.TotalPriceIncludingVat : x.TotalPriceExcludingVat);
            }

            return new CartViewModel
            {
                GrandTotal = currency.Format(cartContext.Cart.Order.GrandTotal, true, CultureInfo.CurrentCulture),
                OrderTotal = currency.Format(orderTotal, true, CultureInfo.CurrentCulture),
                Discount = currency.Format(discount, true, CultureInfo.CurrentCulture),
                DeliveryCost = currency.Format(deliveryCost, true, CultureInfo.CurrentCulture),
                PaymentCost = currency.Format(paymentCost, true, CultureInfo.CurrentCulture),
                Vat = currency.Format(cartContext.Cart.Order.TotalVat, true, CultureInfo.CurrentCulture),
                Quantity = decimal.Round(quantity, 0).ToString(CultureInfo.CurrentCulture),
                OrderRows = orderRows,
                DiscountRows = discountRows,
                CheckoutUrl = checkoutPage?.Href,
            };
        }

        private Dictionary<string, ProductDiscountMap> BuildProductDiscountMap(SalesOrder salesOrder)
        {
            var result = new Dictionary<string, ProductDiscountMap>();

            var productDiscounts = salesOrder.DiscountInfo.Where(x => x.ProductDiscount).ToDictionary(x => x.ResultOrderRowSystemId);
            foreach (var row in salesOrder.Rows.Where(x => productDiscounts.ContainsKey(x.SystemId) && x.ArticleNumber is not null))
            {
                if (result.TryGetValue(row.ArticleNumber, out var item))
                {
                    result[row.ArticleNumber] = item with
                    {
                        ExcludingVat = item.ExcludingVat + row.UnitPriceExcludingVat,
                        IncludingVat = item.IncludingVat + row.UnitPriceIncludingVat
                    };
                }
                else
                {
                    result[row.ArticleNumber] = new ProductDiscountMap(
                        row.UnitPriceExcludingVat,
                        row.UnitPriceIncludingVat);
                }
            }

            return result;
        }

        private List<OrderRowViewModel> BuildDiscountRows(SalesOrder salesOrder, ISet<string> discountTypes, bool includeVat, Currency currency)
        {
            var discountRows = new List<OrderRowViewModel>();
            var discountInfos = salesOrder.DiscountInfo
                .Where(x => discountTypes.Contains(x.PromotionDiscountType))
                .Select(x => x.ResultOrderRowSystemId)
                .ToHashSet();
            if (discountInfos.Count > 0)
            {
                foreach (var discountRow in salesOrder.Rows.Where(x => discountInfos.Contains(x.SystemId)))
                {
                    var discountPrice = currency.Format(
                        includeVat ? discountRow.TotalIncludingVat : discountRow.TotalExcludingVat,
                        true,
                        CultureInfo.CurrentUICulture);
                    discountRows.Add(new OrderRowViewModel
                    {
                        Name = discountRow.Description,
                        Price = discountPrice,
                        Quantity = discountRow.Quantity,
                        TotalPrice = discountPrice,
                        TotalPriceIncludingVat = discountRow.TotalIncludingVat,
                        TotalPriceExcludingVat = discountRow.TotalExcludingVat
                    });
                }
            }

            return discountRows;
        }

        private OrderRowViewModel BuildOrderRow(OrderRow orderRow, Currency currency, bool includeVat, Dictionary<string, ProductDiscountMap> productDiscountMap)
        {
            var variant = _variantService.Get(orderRow.ArticleNumber);
            var baseProduct = _baseProductService.Get(variant?.BaseProductSystemId ?? Guid.Empty);

            var name = variant?.Localizations.CurrentCulture.Name.NullIfWhiteSpace()
                ?? baseProduct?.Localizations.CurrentCulture.Name.NullIfWhiteSpace()
                ?? orderRow.ArticleNumber;
            var url = variant is null ? null : _urlService.GetUrl(variant);
            var image = (variant?.Fields.GetValue<IList<Guid>>(SystemFieldDefinitionConstants.Images)?.FirstOrDefault()
                ?? baseProduct?.Fields.GetValue<IList<Guid>>(SystemFieldDefinitionConstants.Images)?.FirstOrDefault())
                .MapTo<ImageModel>();

            var price = GetPriceModel(orderRow, currency, includeVat, productDiscountMap);
            var totalPrice = new ProductPriceModel.PriceItem(decimal.MinusOne, orderRow.TotalExcludingVat, orderRow.VatRate, orderRow.TotalIncludingVat)
            {
                FormatPrice = b => currency.Format(includeVat ? orderRow.TotalIncludingVat : orderRow.TotalExcludingVat, b, CultureInfo.CurrentUICulture)
            };

            var totalCampaignPrice = GetTotalCampaignPrice(price.CampaignPrice, currency, orderRow, includeVat);

            var unitOfMeasurement = orderRow.UnitOfMeasurementSystemId.HasValue
                ? _unitOfMeasurementService.Get(orderRow.UnitOfMeasurementSystemId.Value)
                : null;
            var unitOfMeasurementFormatString = $"0.{new string('0', unitOfMeasurement?.DecimalDigits ?? 0)}";

            var model = new OrderRowViewModel
            {
                ArticleNumber = orderRow.ArticleNumber,
                Name = name,
                RowSystemId = orderRow.SystemId,
                Quantity = orderRow.Quantity,
                QuantityString = orderRow.Quantity.ToString(unitOfMeasurementFormatString, CultureInfo.CurrentUICulture.NumberFormat).Replace(",", "."),
                Url = url,
                Image = image?.GetUrlToImage(Size.Empty, new Size(200, 120)).Url,
                TotalPrice = totalPrice.FormatPrice(true),
                TotalCampaignPrice = totalCampaignPrice is null ? string.Empty : totalCampaignPrice.FormatPrice(true),
                Price = price.Price.FormatPrice(true),
                CampaignPrice = price.CampaignPrice?.FormatPrice(true),
                // Is freegift have OrderRowType = Product and SystemGenerated = true.
                IsFreeGift = orderRow.SystemGenerated,
                TotalPriceIncludingVat = totalPrice.PriceWithVat,
                TotalPriceExcludingVat = totalPrice.Price,
                TotalCampaignPriceIncludingVat = totalCampaignPrice is null ? null : totalCampaignPrice.PriceWithVat,
                TotalCampaignPriceExcludingVat = totalCampaignPrice is null ? null : totalCampaignPrice.Price
            };

            return model;
        }

        private ProductPriceModel.PriceItem GetTotalCampaignPrice(ProductPriceModel.PriceItem campaignPrice, Currency currency, OrderRow orderRow, bool includeVat)
        {
            if (campaignPrice is null)
            {
                return null;
            }

            var totalExcludingVat = campaignPrice.Price * orderRow.Quantity;
            var totalIncludingVat = campaignPrice.PriceWithVat * orderRow.Quantity;
            return new ProductPriceModel.PriceItem(decimal.MinusOne, totalExcludingVat, orderRow.VatRate, totalIncludingVat)
            {
                FormatPrice = b => currency.Format(includeVat ? totalIncludingVat : totalExcludingVat, b, CultureInfo.CurrentUICulture)
            };
        }

        private ProductPriceModel GetPriceModel(OrderRow orderRow, Currency currency, bool includeVat, Dictionary<string, ProductDiscountMap> productDiscountMap)
        {
            ProductPriceModel.PriceItem campaignPrice = null;

            if (productDiscountMap.TryGetValue(orderRow.ArticleNumber, out var discountedPrice))
            {
                decimal priceExcludingVat = orderRow.UnitPriceExcludingVat + discountedPrice.ExcludingVat;
                decimal priceIncludingVat = orderRow.UnitPriceIncludingVat + discountedPrice.IncludingVat;
                campaignPrice = new ProductPriceModel.PriceItem(decimal.MinusOne, priceExcludingVat, orderRow.VatRate, priceIncludingVat)
                {
                    FormatPrice = b => currency.Format(includeVat ? priceIncludingVat : priceExcludingVat, b, CultureInfo.CurrentUICulture)
                };
            }
            var price = new ProductPriceModel.PriceItem(decimal.MinusOne, orderRow.UnitPriceExcludingVat, orderRow.VatRate, orderRow.UnitPriceIncludingVat)
            {
                FormatPrice = b => currency.Format(includeVat ? orderRow.UnitPriceIncludingVat : orderRow.UnitPriceExcludingVat, b, CultureInfo.CurrentUICulture)
            };
            return new ProductPriceModel
            {
                CampaignPrice = campaignPrice,
                Price = price
            };
        }

        private bool IncludeVat(Cart cart)
        {
            if (cart.Order.ChannelSystemId.HasValue)
            {
                var channel = _channelService.Get(cart.Order.ChannelSystemId.Value);
                if (channel != null)
                {
                    return channel.ShowPricesWithVat;
                }
            }

            return _requestModelAccessor.RequestModel.ChannelModel.Channel.ShowPricesWithVat;
        }
    }
}
