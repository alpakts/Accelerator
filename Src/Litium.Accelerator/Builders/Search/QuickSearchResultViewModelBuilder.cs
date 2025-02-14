using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Searchers;
using Litium.Accelerator.ViewModels.Search;
using Litium.Web;

namespace Litium.Accelerator.Builders.Search
{
    public class QuickSearchResultViewModelBuilder : IViewModelBuilder<QuickSearchResultViewModel>
    {
        private readonly IEnumerable<BaseSearcher> _searchers;
        private readonly RequestModelAccessor _requestModelAccessor;

        public QuickSearchResultViewModelBuilder(IEnumerable<BaseSearcher> searchers, RequestModelAccessor requestModelAccessor)
        {
            _searchers = searchers;
            _requestModelAccessor = requestModelAccessor;
        }

        public virtual async Task<QuickSearchResultViewModel> BuildAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query?.Trim()))
            {
                return new QuickSearchResultViewModel();
            }
            var result = new List<SearchItem>();
            var website = _requestModelAccessor.RequestModel.WebsiteModel;
            _requestModelAccessor.RequestModel.SearchQuery.CategorySystemId = null;
            _requestModelAccessor.RequestModel.SearchQuery.ProductListSystemId = null;
            foreach (var searcher in _searchers.OrderBy(s => s.SortOrder))
            {
                var searchResult = await searcher.QueryCompactAsync(query);
                if (searchResult == null || searchResult.Total <= 0)
                {
                    continue;
                }

                var items = searcher.ToSearchItems(searchResult.Items.Value).ToList();
                var categoryStr = website.Texts.GetValue("quicksearchheader." + searcher.ModelKey) ?? searcher.ModelKey;
                items.ForEach(c => c.Category = categoryStr);

                result.AddRange(items);
            }
            
            if (result.Any())
            {
                result.Add(new SearchItem()
                {
                    Category = "ShowAll",
                    Name = "search.showall".AsWebsiteText(),
                    ShowAll = true
                });
            }
            else
            {
                result.Add(new SearchItem()
                {
                    Category = "NoHit",
                    Name = "search.nohit".AsWebsiteText(),
                });
            }

            return new QuickSearchResultViewModel() { Results = result };
        }
    }
}
