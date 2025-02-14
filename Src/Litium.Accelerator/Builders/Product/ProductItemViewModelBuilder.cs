using System;
using System.Linq;
using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Services;
using Litium.Accelerator.ViewModels.Product;
using Litium.FieldFramework;
using Litium.Globalization;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Sales;
using Litium.Web.Models;
using Litium.Web.Models.Products;

namespace Litium.Accelerator.Builders.Product
{
    public class ProductItemViewModelBuilder : IViewModelBuilder<ProductItemViewModel>
    {
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly ProductPriceModelBuilder _productPriceModelBuilder;
        private readonly FieldDefinitionService _fieldDefinitionService;
        private readonly StockService _stockService;
        private readonly ProductModelBuilder _productModelBuilder;
        private readonly CartContextAccessor _cartContextAccessor;
        private readonly CurrencyService _currencyService;
        private readonly CountryService _countryService;

        public ProductItemViewModelBuilder(RequestModelAccessor requestModelAccessor,
            ProductPriceModelBuilder productPriceModelBuilder,
            FieldDefinitionService fieldDefinitionService,
            StockService stockService,
            ProductModelBuilder productModelBuilder,
            CartContextAccessor cartContextAccessor,
            CurrencyService currencyService,
            CountryService countryService)
        {
            _requestModelAccessor = requestModelAccessor;
            _productPriceModelBuilder = productPriceModelBuilder;
            _fieldDefinitionService = fieldDefinitionService;
            _stockService = stockService;
            _productModelBuilder = productModelBuilder;
            _cartContextAccessor = cartContextAccessor;
            _currencyService = currencyService;
            _countryService = countryService;
        }

        public virtual ProductItemViewModel Build(Variant variant)
        {
            var productModel = _productModelBuilder.BuildFromVariant(variant);
            return productModel == null ? null : Build(productModel);
        }

        public virtual ProductItemViewModel Build(ProductModel productModel, bool inProductListPage = true, Category category = default)
        {
            var cartContext = _cartContextAccessor.CartContext;
            var currency = (cartContext == null) ? _currencyService.Get(_requestModelAccessor.RequestModel.CountryModel.Country.CurrencySystemId)
                                                 : _currencyService.Get(cartContext.CurrencyCode);
            var country = (cartContext == null) ? _requestModelAccessor.RequestModel.CountryModel.Country
                                                : _countryService.Get(cartContext.CountryCode);
            var websiteModel = _requestModelAccessor.RequestModel.WebsiteModel;
            var productPriceModel = _productPriceModelBuilder.Build(productModel.SelectedVariant, currency, _requestModelAccessor.RequestModel.ChannelModel.Channel, country);

            var productName = productModel.GetValue<string>(SystemFieldDefinitionConstants.Name);
            var images = productModel.SelectedVariant.Fields.GetValue<IList<Guid>>(SystemFieldDefinitionConstants.Images)?.Select(guid => guid.MapTo<ImageModel>())?.ToList();
            if (images != null)
            {
                foreach (var item in images)
                {
                    if (string.IsNullOrEmpty(item.Alt))
                    {
                        item.Alt = productName;
                    }
                }
            }

            return new ProductItemViewModel
            {
                Id = productModel.SelectedVariant.Id,
                Price = productPriceModel,
                StockStatusDescription = _stockService.GetStockStatusDescription(productModel.SelectedVariant),
                Currency = currency,
                IsInStock = _stockService.HasStock(productModel.SelectedVariant),
                Images = images,
                Color = _fieldDefinitionService.Get<ProductArea>("Color").GetTranslation(productModel.GetValue<string>("Color")),
                Size = _fieldDefinitionService.Get<ProductArea>("Size").GetTranslation(productModel.GetValue<string>("Size")),
                Brand = _fieldDefinitionService.Get<ProductArea>("Brand").GetTranslation(productModel.GetValue<string>("Brand")),
                Description = productModel.GetValue<string>(SystemFieldDefinitionConstants.Description),
                Name = productName,
                Url = productModel.GetUrl(websiteModel.SystemId, channelSystemId: _requestModelAccessor.RequestModel.ChannelModel.SystemId, currentCategory: category),
                QuantityFieldId = Guid.NewGuid().ToString(),
                ShowBuyButton = websiteModel.GetValue<bool>(AcceleratorWebsiteFieldNameConstants.ShowBuyButton),
                ShowQuantityField = inProductListPage ? websiteModel.GetValue<bool>(AcceleratorWebsiteFieldNameConstants.ShowQuantityFieldProductList)
                                                      : websiteModel.GetValue<bool>(AcceleratorWebsiteFieldNameConstants.ShowQuantityFieldProductPage),
                UseVariantUrl = productModel.UseVariantUrl
            };
        }
    }
}
