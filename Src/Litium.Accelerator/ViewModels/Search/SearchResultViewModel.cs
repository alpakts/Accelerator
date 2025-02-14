using Litium.Accelerator.ViewModels.Product;
using System.Collections.Generic;
using System.Linq;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Search
{
    /// <summary>
    /// Search result view model
    /// </summary>
    public class SearchResultViewModel : IViewModel
    {
        /// <summary>
        /// The search term
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Check search query contain filter of not
        /// </summary>
        public bool ContainsFilter { get; set; }

        /// <summary>
        /// The result of searched products
        /// </summary>
        public IEnumerable<ProductItemViewModel> Products { get; set; } = Enumerable.Empty<ProductItemViewModel>();

        /// <summary>
        /// The pagination of Products
        /// </summary>
        public PaginationViewModel Pagination { get; set; }

        /// <summary>
        /// The other result of searching (ex: pages, brands, categories)
        /// </summary>
        public SearchResult OtherSearchResult { get; set; }
    }
}
