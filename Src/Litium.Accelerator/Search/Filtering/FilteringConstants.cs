using System;

namespace Litium.Accelerator.Search.Filtering
{
    public static class FilteringConstants
    {
        public const string FilterNews = "#News";
        public const string FilterPrice = "#Price";
        public const string FilterProductCategories = "#Categories";

        public static string GetMostSoldTagName(Guid webSiteId)
        {
            return "most-sold-" + webSiteId.ToString("N");
        }

        internal static string GetCampaignTagName(Guid campaignId)
        {
            return "campaign-" + campaignId.ToString("N") + "-price-incl_vat";
        }
    }
}
