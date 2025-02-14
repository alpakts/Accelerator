using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Common;
using Litium.Globalization;
using Litium.Products;
using Litium.Products.StockStatusCalculator;
using Litium.Runtime.DependencyInjection;
using Litium.Sales;
using Litium.Security;
using Microsoft.Extensions.Logging;

namespace Litium.Accelerator.Services
{
    [Service(ServiceType = typeof(StockService))]
    internal class StockServiceImpl : StockService
    {
        private readonly ILogger<StockServiceImpl> _logger;
        private readonly IStockStatusCalculator _stockStatusCalculator;
        private readonly CartContextAccessor _cartContextAccessor;
        private readonly CountryService _countryService;
        private readonly SecurityContextService _securityContextService;
        private readonly KeyLookupService _keyLookupService;
        private readonly InventoryItemService _inventoryItemService;

        public StockServiceImpl(
            ILogger<StockServiceImpl> logger,
            IStockStatusCalculator stockStatusCalculator,
            CartContextAccessor cartContextAccessor,
            CountryService countryService,
            SecurityContextService securityContextService,
            KeyLookupService keyLookupService,
            InventoryItemService inventoryItemService)
        {
            _logger = logger;
            _stockStatusCalculator = stockStatusCalculator;
            _cartContextAccessor = cartContextAccessor;
            _countryService = countryService;
            _securityContextService = securityContextService;
            _keyLookupService = keyLookupService;
            _inventoryItemService = inventoryItemService;
        }

        public override string GetStockStatusDescription(Variant variant, string sourceId = null)
        {
            return GetStockStatus(variant, sourceId)?.Description ?? string.Empty;
        }

        public override bool HasStock(Variant variant, string sourceId = null)
        {
            var stock = GetStockStatus(variant, sourceId);
            return stock?.InStockQuantity.HasValue == true && stock.InStockQuantity > 0m;
        }

        public override void ReduceStock(SalesOrder order)
        {
            if (order is null)
            {
                return;
            }

            _logger.LogTrace("Reducing the stock for sales order {Id}", order.Id);

            var inventories = _stockStatusCalculator.GetInventories(new StockStatusCalculatorArgs
            {
                CountrySystemId = _countryService.Get(order?.CountryCode)?.SystemId,
                UserSystemId = order.CustomerInfo.PersonSystemId.GetValueOrDefault(),
            });

            var articlesPurchased = from o in order.Rows
                                    where o.OrderRowType == OrderRowType.Product
                                    group o by o.ArticleNumber
                                    into g
                                    select new { ArticleNumber = g.Key, Quantity = g.Sum(p => p.Quantity) };

            foreach (var item in articlesPurchased)
            {
                if (_keyLookupService.TryGetSystemId<Variant>(item.ArticleNumber, out var variantSystemId))
                {
                    var inventoryItems = _inventoryItemService.GetByVariant(variantSystemId);
                    var inventorySystemIds = new HashSet<Guid>(inventories.Select(x => x.SystemId));
                    var stockItems = inventoryItems.Where(x => inventorySystemIds.Contains(x.InventorySystemId));
                    var stock = (stockItems.FirstOrDefault(x => x?.InStockQuantity > 0) ?? stockItems.FirstOrDefault())?.MakeWritableClone();
                    //this will set the stock quantities to negative values, if purchased more than the available stocks.
                    //we expect this to be correct to show how much deficit is there for the given article.
                    if (stock is not null)
                    {
                        stock.InStockQuantity -= item.Quantity;
                        using (_securityContextService.ActAsSystem("OrderUtilities.ReduceStockBalance"))
                        {
                            try
                            {
                                _inventoryItemService.Update(stock);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Could not reduce stock for variant '{ArticleNumber}' with '{Quantity}'.", item.ArticleNumber, item.Quantity);
                            }
                        }
                    }
                }
            }
        }

        private StockStatusCalculatorResult GetStockStatus(Variant variant, string sourceId)
        {
            var cartContext = _cartContextAccessor.CartContext;

            var calculatorArgs = new StockStatusCalculatorArgs
            {
                UserSystemId = _securityContextService.GetIdentityUserSystemId().GetValueOrDefault(),
                SourceId = sourceId,
                CountrySystemId = _countryService.Get(cartContext?.CountryCode)?.SystemId
            };
            var calculatorItemArgs = new StockStatusCalculatorItemArgs
            {
                VariantSystemId = variant.SystemId,
                Quantity = decimal.One
            };

            return _stockStatusCalculator.GetStockStatuses(calculatorArgs, calculatorItemArgs).TryGetValue(variant.SystemId, out StockStatusCalculatorResult calculatorResult)
                ? calculatorResult
                : null;
        }
    }
}
