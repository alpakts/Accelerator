using Litium.Accelerator.ViewModels.Framework;
using Litium.Accelerator.ViewModels.Product;
using System.Collections.Generic;

namespace Litium.Accelerator.ViewModels.Search
{
    public class FacetSearchResult
    {
        public IEnumerable<FacetGroupFilter> FacetFilters { get; set; }
        public CategoryFilteringViewModel SortCriteria { get; set; }
        public SubNavigationLinkModel SubNavigation { get; set; }
        public string ProductsView { get; set; }
        public string NavigationTheme { get; set; }
    }
}
