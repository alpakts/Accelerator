using System.Globalization;
using Litium.Products.StockStatusCalculator;
using Litium.Web.Routing;
using Litium.Websites;
using Microsoft.Extensions.Localization;

namespace Litium.Accelerator.Services
{
    internal class StockStatusMessageServiceImpl : IStockStatusMessageService
    {
        private readonly IStringLocalizer _stringLocalizer;
        private readonly RouteRequestLookupInfoAccessor _routeRequestLookupInfoAccessor;
        private readonly WebsiteService _websiteService;

        public StockStatusMessageServiceImpl(
            IStringLocalizer<IStockStatusMessageService> stringLocalizer,
            RouteRequestLookupInfoAccessor routeRequestLookupInfoAccessor,
            WebsiteService websiteService)
        {
            _stringLocalizer = stringLocalizer;
            _routeRequestLookupInfoAccessor = routeRequestLookupInfoAccessor;
            _websiteService = websiteService;
        }

        public StockStatusCalculatorResult Populate(StockStatusCalculatorArgs calculatorArgs, StockStatusCalculatorItemArgs calculatorItemArgs, StockStatusCalculatorResult result)
        {
            var key = (calculatorItemArgs.Quantity == 0 ? result.InStockQuantity > 0 : result.InStockQuantity >= calculatorItemArgs.Quantity) ? "stock.instockwithoutquantity" : "stock.outofstockwithoutquantity";

            var channel = _routeRequestLookupInfoAccessor.RouteRequestLookupInfo?.Channel;
            if (channel?.WebsiteSystemId != null)
            {
                var website = _websiteService.Get(channel.WebsiteSystemId.Value);
                var value = website.Texts.GetValue(key, CultureInfo.CurrentUICulture);
                if (value != null)
                {
                    result.Description = string.Format(value, result.InStockQuantity);
                }
            }

            if (result.Description == null)
            {
                string value = _stringLocalizer.GetString(key);
                if (value != null)
                {
                    result.Description = string.Format(value, result.InStockQuantity);
                }
            }

            if (result.Description == null)
            {
                result.Description = result.InStockQuantity.ToString();
            }

            return result;
        }
    }
}
