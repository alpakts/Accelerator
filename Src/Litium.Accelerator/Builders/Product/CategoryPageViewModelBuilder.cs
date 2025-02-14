using System;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Builders.LandingPage;
using Litium.Accelerator.Caching;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Search;
using Litium.Accelerator.ViewModels;
using Litium.Accelerator.ViewModels.Product;
using Litium.Accelerator.ViewModels.Search;
using Litium.FieldFramework.FieldTypes;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Websites;

namespace Litium.Accelerator.Builders.Product
{
    public class CategoryPageViewModelBuilder : IViewModelBuilder<CategoryPageViewModel>
    {
        private readonly CategoryService _categoryService;
        private readonly LandingPageViewModelBuilder _landingPageViewModelBuilder;
        private readonly ProductItemViewModelBuilder _productItemBuilder;
        private readonly ProductSearchService _productSearchService;
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly PageByFieldTemplateCache<LandingPageByFieldTemplateCache> _landingPageByFieldTemplateCache;

        public CategoryPageViewModelBuilder(CategoryService categoryService,
            ProductItemViewModelBuilder productItemBuilder,
            LandingPageViewModelBuilder landingPageViewModelBuilder,
            ProductSearchService productSearchService,
            RequestModelAccessor requestModelAccessor,
            PageByFieldTemplateCache<LandingPageByFieldTemplateCache> landingPageByFieldTemplateCache)
        {
            _categoryService = categoryService;
            _productItemBuilder = productItemBuilder;
            _landingPageViewModelBuilder = landingPageViewModelBuilder;
            _productSearchService = productSearchService;
            _requestModelAccessor = requestModelAccessor;
            _landingPageByFieldTemplateCache = landingPageByFieldTemplateCache;
        }

        public async Task<CategoryPageViewModel> BuildAsync(Guid categorySystemId, DataFilterBase dataFilter = null)
        {
            if (categorySystemId == Guid.Empty)
            {
                return null;
            }
            var category = _categoryService.Get(categorySystemId);
            if (category == null)
            {
                return null;
            }
            var pageModel = new CategoryPageViewModel() { SystemId = categorySystemId };
            BuildFields(pageModel, category, dataFilter?.Culture);
            await BuildProducts(pageModel, category);
            BuildBlocks(pageModel, category);
            BuildAdditionProperties(pageModel, category);
            return pageModel;
        }

        private void BuildAdditionProperties(CategoryPageViewModel pageModel, Category entity)
        {
            var navigationType = _requestModelAccessor.RequestModel.WebsiteModel.GetNavigationType();
            var searchQuery = _requestModelAccessor.RequestModel.SearchQuery;
            var isFilterType = navigationType == NavigationType.Filter;
            var hasSections = pageModel.Blocks != null && pageModel.Blocks.Count > 0 && !searchQuery.ContainsFilter();

            pageModel.Name = entity.Localizations.CurrentCulture.Name;
            pageModel.Description = entity.Localizations.CurrentCulture.Description;
            pageModel.ShowRegularHeader = isFilterType ? !hasSections && !searchQuery.ContainsFilter() : !hasSections;
            pageModel.ShowFilterHeader = isFilterType && searchQuery.ContainsFilter();
            pageModel.ShowSections = hasSections;
        }

        private static void BuildFields(CategoryPageViewModel pageModel, Category entity, string culture)
        {
            var fields = entity.Fields;
            pageModel.Name = fields.GetName(culture);
            pageModel.Description = fields.GetDescription(culture);
            pageModel.Images = fields.GetImageUrls();
        }

        private async Task BuildProducts(CategoryPageViewModel pageModel, Category category)
        {
            var searchQuery = _requestModelAccessor.RequestModel.SearchQuery.Clone();
            if (searchQuery.PageSize == null)
            {
                var pageSize = _requestModelAccessor.RequestModel.WebsiteModel.GetValue<int?>(AcceleratorWebsiteFieldNameConstants.ProductsPerPage) ?? DefaultWebsiteFieldValueConstants.ProductsPerPage;
                searchQuery.PageSize = pageSize;
            }

            var searchResults = await _productSearchService.SearchAsync(searchQuery, searchQuery.Tags, true, true, true);
            if (searchResults == null)
            {
                pageModel.Pagination = new PaginationViewModel(0, 1);
                return;
            }

            pageModel.Products = searchResults.Items.Value.Cast<ProductSearchResult>().Select(c => _productItemBuilder.Build(c.Item, category: category)).ToList();
            pageModel.Pagination = new PaginationViewModel(searchResults.Total, searchQuery.PageNumber, searchResults.PageSize);
        }

        private void BuildBlocks(CategoryPageViewModel pageModel, Category entity)
        {
            if (_landingPageByFieldTemplateCache.TryGetPage(
                page => page.Fields.GetValue<PointerItem>(PageFieldNameConstants.CategoryPointer)?.EntitySystemId == entity.SystemId,
                out var landingPage))
            {
                var landingPageModel = _landingPageViewModelBuilder.Build(landingPage.MapTo<PageModel>());
                pageModel.Blocks = landingPageModel?.Blocks;
            }
        }
    }
}
