using System;
using System.Globalization;
using System.Linq;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Utilities;
using Litium.Accelerator.ViewModels.Order;
using Litium.ComponentModel;
using Litium.Customers;
using Litium.FieldFramework;
using Litium.Globalization;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Sales;
using Litium.Tagging;
using Litium.Web;
using Litium.Web.Models;
using Litium.Web.Models.Products;
using Litium.Web.Models.Websites;
using Litium.Websites;
using Litium.StateTransitions;
using System.Collections.Generic;
using Litium.Sales.DiscountTypes;

namespace Litium.Accelerator.Builders.Order
{
    public class OrderViewModelBuilder : IViewModelBuilder<OrderViewModel>
    {
        private readonly HashSet<string> _discountTypes = new()
        {
            nameof(DiscountedProductPrice),
            nameof(ProductDiscount),
            nameof(BuyXGetCheapestDiscount),
            nameof(MixAndMatch)
        };

        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly FieldDefinitionService _fieldDefinitionService;
        private readonly CountryService _countryService;
        private readonly TaggingService _taggingService;
        private readonly OrderHelperService _orderHelperService;
        private readonly IsoCountryService _isoCountryService;
        private readonly ShippingProviderService _shippingProviderService;
        private readonly StateTransitionsService _stateTransitionsService;
        private readonly PageService _pageServcie;
        private readonly UrlService _urlService;
        private readonly OrganizationService _organizationService;
        private readonly PersonStorage _personStorage;
        private readonly OrderOverviewService _orderOverviewService;
        private readonly ProductModelBuilder _productModelBuilder;
        private readonly VariantService _variantService;
        private readonly UnitOfMeasurementService _unitOfMeasurementService;
        private readonly ChannelService _channelService;
        private readonly CurrencyService _currencyService;

        public OrderViewModelBuilder(
            RequestModelAccessor requestModelAccessor,
            FieldDefinitionService fieldDefinitionService,
            PageService pageServcie,
            UrlService urlService,
            ProductModelBuilder productModelBuilder,
            VariantService variantService,
            UnitOfMeasurementService unitOfMeasurementService,
            OrganizationService organizationService,
            PersonStorage personStorage,
            OrderOverviewService orderOverviewService,
            ChannelService channelService,
            CurrencyService currencyService,
            ShippingProviderService shippingProviderService,
            StateTransitionsService stateTransitionsService,
            CountryService countryService,
            TaggingService taggingService,
            OrderHelperService orderHelperService,
            IsoCountryService isoCountryService)
        {
            _requestModelAccessor = requestModelAccessor;
            _fieldDefinitionService = fieldDefinitionService;
            _pageServcie = pageServcie;
            _urlService = urlService;

            _productModelBuilder = productModelBuilder;
            _variantService = variantService;
            _unitOfMeasurementService = unitOfMeasurementService;
            _organizationService = organizationService;
            _personStorage = personStorage;
            _orderOverviewService = orderOverviewService;
            _channelService = channelService;
            _currencyService = currencyService;
            _countryService = countryService;
            _taggingService = taggingService;
            _orderHelperService = orderHelperService;
            _isoCountryService = isoCountryService;
            _shippingProviderService = shippingProviderService;
            _stateTransitionsService = stateTransitionsService;
        }

        public virtual OrderViewModel Build(Guid id, bool print)
            => Build(_requestModelAccessor.RequestModel.CurrentPageModel, id, print);

        public OrderViewModel Build(PageModel pageModel, Guid id, bool print)
        {
            var model = pageModel.MapTo<OrderViewModel>();

            var order = _orderOverviewService.Get(id);
            if (order != null)
            {
                model.Order = Build(order);
            }

            model.IsPrintPage = print;
            model.OrderHistoryUrl = GetOrderHistoryUrl(pageModel.Page.ParentPageSystemId);
            model.IsBusinessCustomer = _personStorage.CurrentSelectedOrganizationSystemId is not null;

            if (model.IsBusinessCustomer)
            {
                model.HasApproverRole = _personStorage.HasApproverRole;
            }

            return model;
        }

        public OrderDetailsViewModel Build(OrderOverview orderOverview)
        {
            var order = orderOverview.SalesOrder;
            if (order == null)
            {
                return new OrderDetailsViewModel();
            }
            var includeVat = IncludeVat(order);
            var currency = _currencyService.Get(order.CurrencyCode);

            var paymentOptionName = string.Empty;
            var shippingOptionName = string.Empty;
            DateTimeOffset? deliveryDate = null;

            if (order.ChannelSystemId.HasValue)
            {
                var paymentOption = _orderHelperService.GetPaymentOption(order);
                paymentOptionName = paymentOption?.Name.NullIfWhiteSpace() ?? paymentOption?.Id?.ToString();

                var shippingOption = _orderHelperService.GetShippingOption(order);
                shippingOptionName = shippingOption?.Name.NullIfWhiteSpace() ?? shippingOption?.Id?.ToString();
            }

            var shippingInfo = order.ShippingInfo.FirstOrDefault();
            if (shippingInfo != null)
            {
                var shippingOption = _shippingProviderService.Get(shippingInfo.ShippingOption.ProviderId)?.Options.FirstOrDefault(x => x.Id == shippingInfo.ShippingOption.OptionId);
                if (shippingOption != null)
                {
                    deliveryDate = order.OrderDate.AddDays(shippingOption.DeliveryTimeInDays);
                }
            }

            var orderState = _stateTransitionsService.GetState<SalesOrder>(order.SystemId);
            var statusTranslation = ("sales.order.status." + orderState).AsWebsiteText();
            var orderTotalFee = includeVat ? order.TotalFeesIncludingVat : order.TotalFeesExcludingVat;
            decimal orderTotalDiscountAmount = order.GetNonProductDiscountTotal(includeVat);
            var orderTotalDeliveryCost = includeVat ? order.ShippingCostIncludingVat : order.ShippingCostExcludingVat;
            var totalVat = order.TotalVat;
            var grandTotal = order.GrandTotal;
            var shippingAddress = order.ShippingInfo.FirstOrDefault()?.ShippingAddress;

            var orderDetails = new OrderDetailsViewModel
            {
                OrderId = order.SystemId,
                ExternalOrderID = order.Id,
                OrderDate = order.OrderDate.ToString("d"),
                OrderStatus = orderState,
                Status = statusTranslation,
                OrderTotalFee = currency.Format(orderTotalFee, true, CultureInfo.CurrentUICulture),
                OrderTotalDiscountAmount = currency.Format(orderTotalDiscountAmount, true, CultureInfo.CurrentUICulture),
                OrderTotalDeliveryCost = currency.Format(orderTotalDeliveryCost, true, CultureInfo.CurrentUICulture),
                OrderTotalVat = currency.Format(totalVat, true, CultureInfo.CurrentUICulture),
                OrderGrandTotal = currency.Format(grandTotal, true, CultureInfo.CurrentUICulture),
                OrderRows = order.Rows.Where(x => x.OrderRowType == OrderRowType.Product).Select(x => BuildOrderRow(x, includeVat, currency, order)).ToList(),
                DiscountRows = BuildDiscountRows(order, _discountTypes, includeVat, currency),
                PaymentMethod = paymentOptionName,
                DeliveryMethod = shippingOptionName,
                ActualDeliveryDate = deliveryDate,
                Deliveries = order.ShippingInfo.Select(x => BuildDeliveryRow(orderOverview, x, includeVat, currency)).ToList(),
                CustomerInfo = new OrderDetailsViewModel.CustomerInfoModel
                {
                    CustomerNumber = order.CustomerInfo?.CustomerNumber,
                    FirstName = order.CustomerInfo?.FirstName,
                    LastName = order.CustomerInfo?.LastName,
                    Address1 = shippingAddress?.Address1,
                    Zip = shippingAddress?.ZipCode,
                    City = shippingAddress?.City,
                    Country = string.IsNullOrEmpty(shippingAddress?.Country) ? string.Empty : _isoCountryService.Get(shippingAddress.Country)?.EnglishName
                },
                MerchantOrganizationNumber = _personStorage.CurrentSelectedOrganization?.Id,
                CompanyName = shippingAddress?.OrganizationName,
                IsWaitingApprove = _taggingService.GetAll<Sales.Order>(order.SystemId).Contains(OrderTaggingConstants.AwaitOrderApproval)
            };

            return orderDetails;
        }

        private OrderDetailsViewModel.OrderRowItem BuildOrderRow(OrderRow orderRow, bool includeVat, Currency currency, SalesOrder order)
        {
            var productModel = _productModelBuilder.BuildFromVariant(_variantService.Get(orderRow.ArticleNumber));

            var unitOfMeasurement = _unitOfMeasurementService.Get(orderRow.UnitOfMeasurementSystemId.GetValueOrDefault());
            var unitOfMeasurementFormatString = $"0.{new string('0', unitOfMeasurement?.DecimalDigits ?? 0)}";

            var isFreeGiftRow = orderRow.SystemGenerated;

            var campaignPrice = GetCampaignPrice(orderRow, order);
            var totalPrice = GetTotalPrice(campaignPrice, includeVat, orderRow, isFreeGiftRow);

            var model = new OrderDetailsViewModel.OrderRowItem
            {
                DeliveryId = orderRow.ShippingInfoSystemId is null ? Guid.Empty : orderRow.ShippingInfoSystemId.Value,
                Brand = productModel == null ? null : _fieldDefinitionService.Get<ProductArea>("Brand")?.GetTranslation(productModel.GetValue<string>("Brand")),
                Name = productModel == null ? orderRow.ArticleNumber : productModel.GetValue<string>(SystemFieldDefinitionConstants.Name),
                QuantityString = $"{orderRow.Quantity.ToString(unitOfMeasurementFormatString, CultureInfo.CurrentUICulture.NumberFormat)} {unitOfMeasurement?.Localizations.CurrentUICulture.Name}",
                PriceInfo = new ProductPriceModel
                {
                    Price = SetFormattedPrice(new ProductPriceModel.PriceItem(0, orderRow.UnitPriceExcludingVat, orderRow.VatRate, orderRow.UnitPriceExcludingVat * (1 + orderRow.VatRate)), includeVat, currency),
                    CampaignPrice = campaignPrice != null ? SetFormattedPrice(campaignPrice, includeVat, currency) : null
                },
                TotalPrice = currency.Format(totalPrice, true, CultureInfo.CurrentUICulture),
                Link = productModel?.SelectedVariant.MapTo<LinkModel>(),
                IsFreeGift = isFreeGiftRow
            };

            return model;
        }

        private List<OrderDetailsViewModel.OrderRowItem> BuildDiscountRows(SalesOrder salesOrder, ISet<string> discountTypes, bool includeVat, Currency currency)
        {
            List<OrderDetailsViewModel.OrderRowItem> discountRows = new List<OrderDetailsViewModel.OrderRowItem>();
            var discountInfos = salesOrder.DiscountInfo.Where(x => discountTypes.Contains(x.PromotionDiscountType));
            if (discountInfos.Count() > 0)
            {
                foreach (var discountInfo in discountInfos)
                {
                    var discountRow = salesOrder.Rows.FirstOrDefault(x => discountInfo.ResultOrderRowSystemId == x.SystemId);
                    if (discountRow != null)
                    {
                        var discountPrice = currency.Format(includeVat ? discountRow.TotalIncludingVat : discountRow.TotalExcludingVat, true, CultureInfo.CurrentUICulture);
                        discountRows.Add(new OrderDetailsViewModel.OrderRowItem
                        {
                            Name = discountRow.Description,
                            TotalPrice = discountPrice
                        });
                    }
                }
            }

            return discountRows;
        }

        private decimal GetTotalPrice(ProductPriceModel.PriceItem campaignPrice, bool includeVat, OrderRow orderRow, bool isFreeGift)
        {
            if (isFreeGift && campaignPrice is not null)
            {
                var totalExcludingVat = campaignPrice.Price * orderRow.Quantity;
                var totalIncludingVat = campaignPrice.PriceWithVat * orderRow.Quantity;
                return includeVat ? totalIncludingVat : totalExcludingVat;
            }

            return includeVat ? orderRow.TotalIncludingVat : orderRow.TotalExcludingVat;
        }

        private ProductPriceModel.PriceItem GetCampaignPrice(OrderRow orderRow, SalesOrder salesOrder)
        {
            ProductPriceModel.PriceItem campaignPrice = null;
            if (salesOrder.Rows.Any(c => c.OrderRowType == OrderRowType.Discount && c.ArticleNumber == orderRow.ArticleNumber))
            {
                bool haveCampaignPrice = false;
                decimal priceExcludingVat = orderRow.UnitPriceExcludingVat;
                decimal priceIncludingVat = orderRow.UnitPriceIncludingVat;
                foreach (var discount in salesOrder.DiscountInfo.Where(x => x.ProductDiscount && x.SourceOrderRowSystemIds != null && x.SourceOrderRowSystemIds.Contains(orderRow.SystemId)))
                {
                    var discountRow = salesOrder.Rows.Single(x => x.SystemId == discount.ResultOrderRowSystemId);
                    if (discountRow.OrderRowType == OrderRowType.Discount && discountRow.ArticleNumber == orderRow.ArticleNumber)
                    {
                        priceExcludingVat += discountRow.UnitPriceExcludingVat;
                        priceIncludingVat += discountRow.UnitPriceIncludingVat;
                        haveCampaignPrice = true;
                    }
                }

                if (haveCampaignPrice)
                {
                    campaignPrice = new ProductPriceModel.PriceItem(0, priceExcludingVat, orderRow.VatRate, priceIncludingVat);
                }
            }
            return campaignPrice;
        }

        private OrderDetailsViewModel.DeliveryItem BuildDeliveryRow(OrderOverview orderOverview, ShippingInfo shippingInfo, bool includeVat, Currency currency)
        {
            var shipmentRows = orderOverview.Shipments?.SelectMany(x => x.Rows.Where(y => y.ShippingInfoSystemId.GetValueOrDefault() == shippingInfo.SystemId));
            var shipmentCost = shipmentRows == null ? 0 :
                    includeVat ? shipmentRows.Select(x => x.TotalIncludingVat).Sum() : shipmentRows.Select(x => x.TotalExcludingVat).Sum();

            var order = orderOverview.SalesOrder;
            var channel = _channelService.Get(order.ChannelSystemId.Value);
            var shippingOptionName = string.Empty;
            if (channel is not null)
            {
                var country = _countryService.Get(order.CountryCode);
                var countryLink = channel.CountryLinks.FirstOrDefault(x => x.CountrySystemId == country.SystemId);

                if (shippingInfo != null)
                {
                    shippingOptionName = countryLink?.ShippingOptions.FirstOrDefault(x => x.Id.Equals(shippingInfo.ShippingOption))?.Name;
                }
            }

            var model = new OrderDetailsViewModel.DeliveryItem
            {
                DeliveryId = shippingInfo.SystemId,
                DeliveryMethodTitle = shippingOptionName,
                DeliveryRowTotalCost = currency.Format(shipmentCost, true, CultureInfo.CurrentUICulture)
            };
            model.Address.MapFrom(shippingInfo.ShippingAddress);

            return model;
        }

        private string GetOrderHistoryUrl(Guid sytemId)
        {
            var orderHistoryPage = _pageServcie.Get(sytemId);
            return _urlService.GetUrl(orderHistoryPage);
        }

        private T SetFormattedPrice<T>(T item, bool shoppingCartIncludeVat, Currency currency) where T : ProductPriceModel.PriceItem
        {
            var price = shoppingCartIncludeVat ? item.PriceWithVat : item.Price;
            item.FormatPrice = x => currency.Format(price, x, CultureInfo.CurrentUICulture);
            return item;
        }

        private bool IncludeVat(SalesOrder order)
        {
            if (order.ChannelSystemId.HasValue)
            {
                var channel = _channelService.Get(order.ChannelSystemId.Value);
                if (channel != null)
                {
                    return channel.ShowPricesWithVat;
                }
            }

            return _requestModelAccessor.RequestModel.ChannelModel.Channel.ShowPricesWithVat; ;
        }
    }
}
