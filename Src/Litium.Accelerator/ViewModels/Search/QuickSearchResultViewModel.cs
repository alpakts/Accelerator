using System.Collections.Generic;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Search
{
    public class QuickSearchResultViewModel : IViewModel
    {
        public IEnumerable<SearchItem> Results { get; set; }
    }
}
