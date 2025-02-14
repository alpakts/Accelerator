using System.Threading.Tasks;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Search;
using Litium.Accelerator.ViewModels.Search;

namespace Litium.Accelerator.Searchers
{
    public class CategorySearcher : BaseSearcher<CategorySearchResult>
    {
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly CategorySearchService _categorySearchService;

        public CategorySearcher(CategorySearchService categorySearchService, RequestModelAccessor requestModelAccessor)
        {
            _requestModelAccessor = requestModelAccessor;
            _categorySearchService = categorySearchService;
        }

        public override int SortOrder => 200;

        public override int PageSize => 10;

        public override string ModelKey => "Categories";

        public override Task<SearchResult> QueryCompactAsync(string query, bool includeScore = false)
        {
            var searchQuery = _requestModelAccessor.RequestModel.SearchQuery.Clone();
            searchQuery.Text = query;
            searchQuery.PageNumber = 1;
            searchQuery.PageSize = PageSize;

            return _categorySearchService.SearchAsync(searchQuery, includeScore: includeScore);
        }
    }
}
