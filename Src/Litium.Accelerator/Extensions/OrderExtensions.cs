using System.Linq;
using SalesOrder = Litium.Sales.SalesOrder;

namespace Litium.Accelerator.Extensions
{
    /// <summary>
    /// Represents an extension for <see cref="SalesOrder"/>.
    /// </summary>
    public static class OrderExtensions
    {
        /// <summary>
        /// Get the total of all discounts that are not product discounts.
        /// </summary>
        /// <param name="salesOrder">The sales order.</param>
        /// <param name="includeVat">Indicate should we include VAT for a discount or not.</param>
        /// <returns><c>decimal</c> The discount</returns>
        public static decimal GetNonProductDiscountTotal(this SalesOrder salesOrder, bool includeVat)
        {
            decimal discount = 0;
            var resultOrderRows = salesOrder.DiscountInfo.Where(x => !x.ProductDiscount).Select(x => x.ResultOrderRowSystemId).ToHashSet();
            if (resultOrderRows.Count > 0)
            {
                var discountRows = salesOrder.Rows.Where(x => resultOrderRows.Contains(x.SystemId));
                discount = includeVat ? discountRows.Sum(x => x.TotalIncludingVat) : discountRows.Sum(x => x.TotalExcludingVat);
            }

            return discount;
        }
    }
}
