using System.Collections.Generic;

namespace Litium.Accelerator.ViewModels.Product
{
    public class VariantInfo : PageViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Images { get; set; }
        public string Description { get; set; }
        public decimal ListPrice { get; set; }
        public decimal VatPercentage { get; set; }
        public decimal CampaignPriceWithVat { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
    }
}
