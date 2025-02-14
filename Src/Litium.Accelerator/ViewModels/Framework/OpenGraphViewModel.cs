using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Framework
{
    public class OpenGraphViewModel : IViewModel
    {
        public string ImageUrl { get; set; }
        public bool ImageVisible { get; set; }
        public string Locale { get; set; }
        public string LogotypeImageUrl { get; set; }
        public bool LogotypeVisible { get; set; }
        public string MetaDescription { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string WebSiteTitle { get; set; }
    }
}
