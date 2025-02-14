using System.Collections.Generic;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Framework
{
    public class BreadCrumbsViewModel : IViewModel
    {
        public List<ContentLinkModel> ContentLinks { get; set; }
    }
}