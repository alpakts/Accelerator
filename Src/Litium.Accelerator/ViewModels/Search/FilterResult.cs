using System.Collections.Generic;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Search
{
    public class FilterResult : IViewModel
    {
        public IList<GroupFilter> Items { get; set; }
    }
}
