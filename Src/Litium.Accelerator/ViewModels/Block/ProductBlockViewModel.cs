using Litium.Accelerator.ViewModels.Product;
using System.Collections.Generic;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Block
{
    public class ProductBlockViewModel : IViewModel
    {
        public string FooterLinkText { get; set; }
        public string FooterLinkUrl { get; set; }
        public List<ProductItemViewModel> Products { get; set; }
        public string Title { get; set; }
    }
}
