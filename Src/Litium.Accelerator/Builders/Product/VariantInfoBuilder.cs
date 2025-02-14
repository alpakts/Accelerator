using System;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Services;
using Litium.Accelerator.ViewModels;
using Litium.Accelerator.ViewModels.Product;
using Litium.Globalization;
using Litium.Media;
using Litium.Products;
using Litium.Sales;
using Litium.Web.Models.Products;

namespace Litium.Accelerator.Builders.Product
{
    public class VariantInfoBuilder : IViewModelBuilder<VariantInfo>
    {
        private readonly VariantService _variantService;
        private readonly ProductPriceModelBuilder _priceModelBuilder;
        private readonly CurrencyService _currencyService;
        private readonly ChannelService _channelService;
        private readonly CountryService _countryService;
        private readonly CartContextAccessor _cartContextAccessor;
        private readonly RequestModelAccessor _requestModelAccessor;

        public VariantInfoBuilder(
            VariantService variantService, 
            ProductPriceModelBuilder priceModelBuilder, 
            CurrencyService currencyService, 
            ChannelService channelService,
            CountryService countryService,
            CartContextAccessor cartContextAccessor,
            RequestModelAccessor requestModelAccessor)
        {
            _variantService = variantService;
            _priceModelBuilder = priceModelBuilder;
            _currencyService = currencyService;
            _channelService = channelService;
            _countryService = countryService;
            _cartContextAccessor = cartContextAccessor;
            _requestModelAccessor = requestModelAccessor;
        }

        public VariantInfo Build(Guid variantSystemId, Guid channelSystemId, DataFilterBase dataFilter = null)
        {
            if (variantSystemId == Guid.Empty)
            {
                return null;
            }
            var entity = _variantService.Get(variantSystemId);
            if (entity == null)
            {
                return null;
            }
            var pageModel = new VariantInfo() { SystemId = variantSystemId };
            var currency = _currencyService.Get(id: ((ProductDataFilter)dataFilter)?.Currency) ?? _currencyService.GetBaseCurrency();
            BuildFields(pageModel, entity, dataFilter?.Culture);
            BuildPrices(pageModel, entity, currency, channelSystemId);
            return pageModel;
        }

        private void BuildFields(VariantInfo pageModel, Variant entity, string culture)
        {
            pageModel.Id = entity.Id;
            var fields = entity.Fields;
            pageModel.Name = fields.GetName(culture);
            pageModel.Description = fields.GetDescription(culture);
            pageModel.Images = fields.GetImageUrls();
            pageModel.Size = fields.GetSize();
            pageModel.Color = fields.GetColor();
        }

        private void BuildPrices(VariantInfo pageModel, Variant entity, Currency currency, Guid channelSystemId)
        {
            var channel = _channelService.Get(channelSystemId);
            var cartContext = _cartContextAccessor.CartContext;
            var country = (cartContext == null) ? _requestModelAccessor.RequestModel.CountryModel.Country
                                                : _countryService.Get(cartContext.CountryCode);
            var productPriceModel = _priceModelBuilder.Build(entity, currency, channel, country);
            if (productPriceModel.Price == null)
            {
                return;
            }
            pageModel.ListPrice = productPriceModel.Price.Price;
            pageModel.VatPercentage = productPriceModel.Price.VatPercentage;
            pageModel.CampaignPriceWithVat = productPriceModel.CampaignPrice.PriceWithVat;
        }
    }
}
