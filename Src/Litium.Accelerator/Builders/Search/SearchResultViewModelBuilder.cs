using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Builders.Product;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Search;
using Litium.Accelerator.ViewModels;
using Litium.Accelerator.ViewModels.Search;

namespace Litium.Accelerator.Builders.Search
{
    public class SearchResultViewModelBuilder : IViewModelBuilder<SearchResultViewModel>
    {
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly ProductSearchService _productSearchService;
        private readonly ProductItemViewModelBuilder _productItemBuilder;
        private readonly PageSearchService _pageSearchService;
        private readonly CategorySearchService _categorySearchService;

        public SearchResultViewModelBuilder(RequestModelAccessor requestModelAccessor, ProductSearchService productSearchService, ProductItemViewModelBuilder productItemBuilder,
           PageSearchService pageSearchService, CategorySearchService categorySearchService)
        {
            _requestModelAccessor = requestModelAccessor;
            _productSearchService = productSearchService;
            _productItemBuilder = productItemBuilder;
            _pageSearchService = pageSearchService;
            _categorySearchService = categorySearchService;
        }

        public virtual async Task<SearchResultViewModel> BuildAsync()
        {
            var searchQuery = _requestModelAccessor.RequestModel.SearchQuery.Clone();
            var searchResult = new SearchResultViewModel() { SearchTerm = searchQuery.Text, ContainsFilter = searchQuery.ContainsFilter() };

            if (!string.IsNullOrWhiteSpace(searchQuery.Text))
            {
                await BuildProductsAsync(searchResult, searchQuery);
                await BuildOtherSearchResultAsync(searchResult, searchQuery);
            }

            return searchResult;
        }

        private async Task BuildProductsAsync(SearchResultViewModel searchResult, SearchQuery searchQuery)
        {
            if (searchQuery.PageSize == null)
            {
                var pageSize = _requestModelAccessor.RequestModel.WebsiteModel.GetValue<int?>(AcceleratorWebsiteFieldNameConstants.ProductsPerPage) ?? DefaultWebsiteFieldValueConstants.ProductsPerPage;
                searchQuery.PageSize = pageSize;
            }

            var searchResults = await _productSearchService.SearchAsync(searchQuery, searchQuery.Tags, true, true, true);

            if (searchResults == null)
            {
                searchResult.Pagination = new PaginationViewModel(0, 1);
                return;
            }

            searchResult.Products = searchResults.Items.Value.Cast<ProductSearchResult>().Select(c => _productItemBuilder.Build(c.Item)).ToList();
            searchResult.Pagination = new PaginationViewModel(searchResults.Total, searchQuery.PageNumber, searchResults.PageSize);
        }

        private async Task BuildOtherSearchResultAsync(SearchResultViewModel searchResult, SearchQuery searchQuery)
        {
            var p = searchQuery.PageSize;
            searchQuery.PageNumber = 1;
            searchQuery.PageSize = 100;

            var pageSearchResult = await _pageSearchService.SearchAsync(searchQuery, includeScore: true);
            var categorySearchResult = await _categorySearchService.SearchAsync(searchQuery, includeScore: true);

            searchQuery.PageSize = p;
            searchResult.OtherSearchResult = new SearchResult
            {
                PageSize = 100,
                Total = (pageSearchResult?.Total ?? 0) + (categorySearchResult?.Total ?? 0),
                Items = new Lazy<IEnumerable<SearchResultItem>>(() => (
                    pageSearchResult == null
                    ? Array.Empty<SearchResultItem>()
                    : pageSearchResult.Items.Value
                    ).Concat(
                        categorySearchResult == null
                        ? Array.Empty<SearchResultItem>()
                        : categorySearchResult.Items.Value).OrderByDescending(x => x.Score))
            };
        }
    }
}
