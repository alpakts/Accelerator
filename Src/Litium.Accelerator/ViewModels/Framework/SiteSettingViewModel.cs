using System;
using Litium.Accelerator.Builders;
using Litium.Web;

namespace Litium.Accelerator.ViewModels.Framework
{
    /// <summary>
    /// Represenst the view model for current website setting.
    /// </summary>
    public class SiteSettingViewModel : IViewModel
    {
        public Guid ChannelSystemId { get; set; }
        public Guid CurrentPageSystemId { get; set; }
        public Guid? ProductCategorySystemId { get; set; }
        public PreviewPageUrlArgs PreviewPageData { get; set; }
    }
}
