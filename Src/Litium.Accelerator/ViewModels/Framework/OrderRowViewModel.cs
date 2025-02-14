using System;

namespace Litium.Accelerator.ViewModels.Framework
{
    public class OrderRowViewModel
    {
        public string ArticleNumber { get; set; }
        public Guid RowSystemId { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public string QuantityString { get; set; }
        public string Price { get; set; }
        public string CampaignPrice { get; set; }
        public string TotalPrice { get; set; }
        public string TotalCampaignPrice { get; set; }
        public string Url { get; set; }
        public string CampaignLink { get; set; }
        public bool IsFreeGift { get; set; }
        public decimal TotalPriceIncludingVat { get; set; }
        public decimal TotalPriceExcludingVat { get; set; }
        public decimal? TotalCampaignPriceIncludingVat { get; set; }
        public decimal? TotalCampaignPriceExcludingVat { get; set; }
    }
}
