using System.ComponentModel.DataAnnotations;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Framework
{
    public class FaviconViewModel : IViewModel
    {
        public enum RelType
        {
            [Display(Name = "shortcut icon")]
            ShortcutIcon,
            [Display(Name = "apple-touch-icon")]
            AppleTouchIcon,
            [Display(Name = "icon")]
            Icon
        }

        public FaviconViewModel(RelType relType)
        {
            Type = relType == RelType.Icon ? "image/png" : string.Empty;
        }

        public string Href { get; set; }
        public string Rel { get; set; }
        public string Size { get; set; }
        public string Type { get; set; }
    }
}
