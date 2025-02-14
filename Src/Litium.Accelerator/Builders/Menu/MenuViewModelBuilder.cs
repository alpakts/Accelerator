using System;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Routing;
using Litium.Accelerator.ViewModels.Menu;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Web.Products.Routing;
using Litium.Web.Routing;

namespace Litium.Accelerator.Builders.Menu
{
    public class MenuViewModelBuilder : IViewModelBuilder<MenuViewModel>
    {
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly RouteRequestInfoAccessor _routeRequestInfoAccessor;

        public MenuViewModelBuilder(RequestModelAccessor requestModelAccessor, RouteRequestInfoAccessor routeRequestInfoAccessor)
        {
            _requestModelAccessor = requestModelAccessor;
            _routeRequestInfoAccessor = routeRequestInfoAccessor;
        }

        public MenuViewModel Build()
        {
            var website = _requestModelAccessor.RequestModel.WebsiteModel;
            var page = _requestModelAccessor.RequestModel.CurrentPageModel;
            var filterNavigation = website.GetNavigationType();

            var showleftColumn = false;
            if (_routeRequestInfoAccessor.RouteRequestInfo?.Data is ProductPageData productCatalogData)
            {
                if (productCatalogData.BaseProductSystemId != null)
                {
                    return new MenuViewModel()
                    {
                        ShowLeftColumn = false
                    };
                }

                showleftColumn = true;
                if (!website.InFirstLevelCategories())
                {
                    showleftColumn = productCatalogData.CategorySystemId.GetValueOrDefault().MapTo<Category>()?.ParentCategorySystemId != Guid.Empty;
                }
            }
            //current page is start page
            else if (page != null && page.Page.ParentPageSystemId != Guid.Empty)
            {
                var pageTypeName = page.GetPageType();
                switch (pageTypeName)
                {
                    case null:
                        return new MenuViewModel()
                        {
                            ShowLeftColumn = false
                        };
                    case PageTemplateNameConstants.Brand:
                        showleftColumn = filterNavigation == NavigationType.Filter && website.InBrandPages();
                        break;
                    case PageTemplateNameConstants.ProductList:
                        showleftColumn = filterNavigation == NavigationType.Filter && website.InProductListPages();
                        break;
                    case PageTemplateNameConstants.SearchResult:
                        showleftColumn = filterNavigation == NavigationType.Filter;
                        break;
                    case PageTemplateNameConstants.Article:
                    case PageTemplateNameConstants.NewsList:
                    case PageTemplateNameConstants.News:
                        showleftColumn = website.InArticlePages();
                        break;
                }
            }

            return new MenuViewModel()
            {
                ShowLeftColumn = showleftColumn
            };
        }
    }
}
