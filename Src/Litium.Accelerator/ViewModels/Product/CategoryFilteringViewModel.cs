using Litium.Accelerator.ViewModels.Search;
using System.Collections.Generic;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Product
{
    public class CategoryFilteringViewModel : IViewModel
    {
        public IEnumerable<ListItem> SortItems { get; set; }
    }
}
