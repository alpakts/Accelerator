using Litium.Accelerator.ViewModels.Search;
using System.Collections.Generic;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Framework
{
    /// <summary>
    /// Represents the view model for client-side context.
    /// </summary>
    public class ClientContextViewModel : IViewModel
    {
        public SiteSettingViewModel SiteSetting { get; set; }
        public CartViewModel Cart { get; set; }
        public NavigationViewModel Navigation { get; set; }
        public bool IsBusinessCustomer { get; set; }
        public bool HasApproverRole { get; set; }
        public List<ListItem> Countries { get; set; }
        public string QuickSearchUrl { get; set; }
        public string RequestVerificationToken { get; set; }
        public IDictionary<string, string> Texts { get; set; }
    }
}
