using System.Collections.Generic;

namespace Litium.Accelerator.ViewModels.Framework
{
    public class CartViewModel : PageViewModel
    {
        public string CheckoutUrl { get; set; }
        public string OrderTotal { get; set; }
        public string Quantity { get; set; }
        public IList<OrderRowViewModel> OrderRows { get; set; }
        public IList<OrderRowViewModel> DiscountRows { get; set; }

        public string Discount { get; set; }
        public string DeliveryCost { get; set; }
        public string PaymentCost { get; set; }
        public string GrandTotal { get; set; }
        public string Vat { get; set; }
    }
}
