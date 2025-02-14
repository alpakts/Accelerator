using System.Collections.Generic;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Product
{
    public class ProductFieldGroupViewModel : IViewModel
    {
        public string GroupId { get; set; }

        public string HtmlGroupId => GroupId?.Replace(" ", "-").ToLowerInvariant() ?? string.Empty;

        public string Name { get; init; }

        public IEnumerable<ProductFieldViewModel> ProductFields { get; init; } 
    }
}
