using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Search;
using Litium.Accelerator.ViewModels.Brand;
using Litium.Accelerator.ViewModels.Product;
using Litium.Accelerator.ViewModels.Search;
using Litium.Web;
using Litium.Data.Queryable;

namespace Litium.Accelerator.Builders.Product
{
    public class CategoryFilteringViewModelBuilder : IViewModelBuilder<CategoryFilteringViewModel>
    {
        private readonly RequestModelAccessor _requestModelAccessor;

        public CategoryFilteringViewModelBuilder(RequestModelAccessor requestModelAccessor)
        {
            _requestModelAccessor = requestModelAccessor;
        }

        public CategoryFilteringViewModel Build(int totalHits)
        {
            if (totalHits > 1)
            {
                return new CategoryFilteringViewModel
                {
                    SortItems = GetSortSelection()
                };
            }

            return null;
        }

        private IEnumerable<ListItem> GetSortSelection()
        {
            var sortList = new List<ListItem>();
            var searchQuery = _requestModelAccessor.RequestModel.SearchQuery.Clone();
            var pageTypeName = _requestModelAccessor.RequestModel.CurrentPageModel.GetPageType();
            if (pageTypeName == PageTemplateNameConstants.Brand)
            {
                searchQuery.Tags.Remove(BrandListViewModel.TagName);
            }
            searchQuery.PageType = pageTypeName;
            searchQuery.PageSystemId = _requestModelAccessor.RequestModel.CurrentPageModel.SystemId;

            if (!string.IsNullOrWhiteSpace(searchQuery.Text) || pageTypeName == PageTemplateNameConstants.SearchResult)
            {
                sortList.Add(new ListItem("sort.byscore".AsWebsiteText(), searchQuery.GetUrlSort(string.Empty, SortDirection.Ascending)));
                sortList.Add(new ListItem("sort.bypopular".AsWebsiteText(), searchQuery.GetUrlSort(SearchQueryConstants.Popular, SortDirection.Ascending)) { Selected = searchQuery.IsSortedBy(SearchQueryConstants.Popular, SortDirection.Ascending) });
            }
            else
            {
                sortList.Add(new ListItem("sort.bypopular".AsWebsiteText(), searchQuery.GetUrlSort(string.Empty, SortDirection.Ascending)));
            }

            sortList.Add(new ListItem("sort.byrecommend".AsWebsiteText(), searchQuery.GetUrlSort(SearchQueryConstants.Recommended, SortDirection.Ascending)) { Selected = searchQuery.IsSortedBy(SearchQueryConstants.Recommended, SortDirection.Ascending) });
            sortList.Add(new ListItem("sort.bynews".AsWebsiteText(), searchQuery.GetUrlSort(SearchQueryConstants.News, SortDirection.Ascending)) { Selected = searchQuery.IsSortedBy(SearchQueryConstants.News, SortDirection.Ascending) });
            sortList.Add(new ListItem("sort.bynameasc".AsWebsiteText(), searchQuery.GetUrlSort(SearchQueryConstants.Name, SortDirection.Ascending)) { Selected = searchQuery.IsSortedBy(SearchQueryConstants.Name, SortDirection.Ascending) });
            sortList.Add(new ListItem("sort.bynamedesc".AsWebsiteText(), searchQuery.GetUrlSort(SearchQueryConstants.Name, SortDirection.Descending)) { Selected = searchQuery.IsSortedBy(SearchQueryConstants.Name, SortDirection.Descending) });
            sortList.Add(new ListItem("sort.bypriceasc".AsWebsiteText(), searchQuery.GetUrlSort(SearchQueryConstants.Price, SortDirection.Ascending)) { Selected = searchQuery.IsSortedBy(SearchQueryConstants.Price, SortDirection.Ascending) });
            sortList.Add(new ListItem("sort.bypricedesc".AsWebsiteText(), searchQuery.GetUrlSort(SearchQueryConstants.Price, SortDirection.Descending)) { Selected = searchQuery.IsSortedBy(SearchQueryConstants.Price, SortDirection.Descending) });

            return sortList;
        }
    }
}
