using System;
using System.Linq;
using Litium.Accelerator.Search;
using Litium.Globalization;
using Litium.Runtime.AutoMapper;
using Litium.Sales;
using Litium.Web.Models.Globalization;
using Litium.Web.Models.Products;
using Litium.Web.Models.Websites;
using Litium.Websites;

namespace Litium.Accelerator.Routing
{
    public abstract class RequestModel
    {
        private readonly Lazy<WebsiteModel> _websiteModel;
        private readonly Lazy<CountryModel> _countryModel;
        private readonly CartContext _cartContext;

        protected RequestModel(
            CartContext cartContext,
            CountryService countryService)
        {
            _cartContext = cartContext;
            _websiteModel = new Lazy<WebsiteModel>(() => ChannelModel.Channel.WebsiteSystemId.GetValueOrDefault().MapTo<Website>().MapTo<WebsiteModel>());
            _countryModel = new Lazy<CountryModel>(() => (countryService.Get(Cart?.Order.CountryCode)?.SystemId ?? ChannelModel.Channel.CountryLinks.FirstOrDefault().CountrySystemId).MapTo<CountryModel>());
        }

        public abstract ChannelModel ChannelModel { get; }
        public abstract PageModel CurrentPageModel { get; }
        public abstract SearchQuery SearchQuery { get; }
        public abstract ProductModel CurrentProductModel { get; }
        public abstract CategoryModel CurrentCategoryModel { get; }
        public virtual WebsiteModel WebsiteModel => _websiteModel.Value;
        public virtual Cart Cart => _cartContext?.Cart;
        public virtual CountryModel CountryModel => _countryModel.Value;
        public DateTimeOffset DateTimeUtc { get; } = DateTimeOffset.UtcNow;
    }
}
