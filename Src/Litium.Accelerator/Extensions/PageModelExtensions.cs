using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Accelerator.Constants;
using Litium.FieldFramework;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Websites;
using Litium.Websites;

namespace Litium.Accelerator.Extensions
{
    public static class PageModelExtensions
    {
        /// <summary>
        /// Get all ancestor pages of current page include itself
        /// </summary>
        /// <param name="pageModel"></param>
        /// <returns></returns>
        public static List<PageModel> GetAncestors(this PageModel pageModel)
        {
            var pageModels = new List<PageModel>();

            while (pageModel != null && pageModel.Page.ParentPageSystemId != Guid.Empty)
            {
                pageModel = pageModel.Page.ParentPageSystemId.MapTo<PageModel>();
                if (pageModel != null)
                {
                    pageModels.Add(pageModel);
                }
            }

            return pageModels;
        }

        /// <summary>
        /// Return Id of Page Field Template
        /// </summary>
        /// <param name="pageModel">The page model</param>
        /// <returns></returns>
        public static string GetPageType(this PageModel pageModel)
        {
            return pageModel.Page.GetPageType();
        }

        /// <summary>
        /// Return Id of Page Field Template
        /// </summary>
        /// <param name="page">The Page</param>
        /// <returns></returns>
        public static string GetPageType(this Page page)
        {
            return page.MapTo<FieldTemplate<WebsiteArea>>()?.Id;
        }

        /// <summary>
        /// Check if page has "Brand" page type
        /// </summary>
        /// <param name="pageModel">The page model</param>
        /// <returns></returns>
        public static bool IsBrandPageType(this PageModel pageModel)
        {
            return pageModel.GetPageType() == PageTemplateNameConstants.Brand;
        }

        /// <summary>
        /// Check if page has "SearchResult" page type
        /// </summary>
        /// <param name="pageModel">The page model</param>
        /// <returns></returns>
        public static bool IsSearchResultPageType(this PageModel pageModel)
        {
            return pageModel.GetPageType() == PageTemplateNameConstants.SearchResult;
        }
        /// <summary>
        /// Check if page has "ProductList" page type
        /// </summary>
        /// <param name="pageModel">The page model</param>
        /// <returns></returns>
        public static bool IsProductListPageType(this PageModel pageModel)
        {
            return pageModel.GetPageType() == PageTemplateNameConstants.ProductList;
        }

        /// <summary>
        /// Sort the pages regarding to the sort order.
        /// </summary>
        /// <param name="source">The pages.</param>
        /// <param name="pageSortOrderType">The sort order.</param>
        /// <returns>Sorted list of pages</returns>
        public static IOrderedEnumerable<Page> Sort(this IEnumerable<Page> source, PageSortOrderType pageSortOrderType)
        {
            if (pageSortOrderType == PageSortOrderType.Manual)
            {
                return source.OrderBy(f => f.SortIndex);
            }
            else if (pageSortOrderType == PageSortOrderType.PublishDate)
            {
                return source.OrderBy(f => f.PublishedAtUtc);
            }
            else if (pageSortOrderType == PageSortOrderType.PublishDateDesc)
            {
                return source.OrderByDescending(f => f.PublishedAtUtc);
            }
            else if (pageSortOrderType == PageSortOrderType.AlphabetDesc)
            {
                return source.OrderByDescending(f => f.Localizations.CurrentUICulture.Name);
            }
            return source.OrderBy(f => f.Localizations.CurrentUICulture.Name);
        }
    }
}
