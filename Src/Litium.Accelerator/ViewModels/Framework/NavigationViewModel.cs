using System.Collections.Generic;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Framework
{
    public class NavigationViewModel : IViewModel
    {
        public ICollection<ContentLinkModel> ContentLinks { get; set; } = new List<ContentLinkModel>();
    }
}
