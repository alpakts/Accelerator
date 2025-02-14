using System.Collections.Generic;

namespace Litium.Accelerator.ViewModels.Search
{
    public class FacetGroupFilter
    {
        public string Label { get; set; }
        public string Id { get; set; }
        public string[] SelectedOptions { get; set; }
        public bool SingleSelect { get; set; }
        public List<FacetFilter> Options { get; set; }
    }
}
