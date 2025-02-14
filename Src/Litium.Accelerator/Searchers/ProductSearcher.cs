using System.Threading.Tasks;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Search;
using Litium.Accelerator.ViewModels.Search;

namespace Litium.Accelerator.Searchers
{
    public class ProductSearcher : BaseSearcher<ProductSearchResult>
    {
        private readonly ProductSearchService _productSearchService;
        private readonly RequestModelAccessor _requestModelAccessor;

        public override int SortOrder => 100;

        public override int PageSize => 20;

        public override string ModelKey => "Products";

        public ProductSearcher(ProductSearchService productSearchService, RequestModelAccessor requestModelAccessor)
        {
            _productSearchService = productSearchService;
            _requestModelAccessor = requestModelAccessor;
        }

        public override async Task<SearchResult> QueryCompactAsync(string query, bool includeScore = false)
        {
            var searchQuery = _requestModelAccessor.RequestModel.SearchQuery.Clone();
            searchQuery.Text = query;
            searchQuery.PageSize = PageSize;

            return await _productSearchService.SearchAsync(searchQuery, searchQuery.Tags, true, true, true);
        }
    }
}
