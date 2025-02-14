using JetBrains.Annotations;
using System.Collections.Generic;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Framework
{
    public class HeadViewModel : IViewModel
    {
        public string Canonical { get; set; }
        [NotNull]
        public List<FaviconViewModel> Favicons { get; set; } = new List<FaviconViewModel>();
        public string MetaDescription { get; set; }
        public string MetaRobots { get; set; }
        public OpenGraphViewModel OpenGraph { get; set; }
        public string Title { get; set; }
        public string TrackingScripts { get; set; }
    }
}
