using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Routing;
using Litium.Accelerator.ViewModels.Framework;
using Litium.FieldFramework;
using Litium.Runtime.AutoMapper;
using Litium.Web;
using Litium.Web.Models.Globalization;
using Litium.Web.Models.Products;

namespace Litium.Accelerator.Builders.Framework
{
    public class BreadCrumbsViewModelBuilder<TViewModel> : IViewModelBuilder<TViewModel>
        where TViewModel : BreadCrumbsViewModel
    {
        private readonly UrlService _urlService;
        private readonly RequestModelAccessor _requestModelAccessor;

        public BreadCrumbsViewModelBuilder(UrlService urlService, RequestModelAccessor requestModelAccessor)
        {
            _urlService = urlService;
            _requestModelAccessor = requestModelAccessor;
        }

        /// <summary>
        /// Build the bread crumbs view model
        /// </summary>
        /// <param name="startLevel">Defines from which level the breadcrumbs will be rendered</param>
        /// <returns>Return BreadCrumbsViewModel</returns>
        public BreadCrumbsViewModel BuildBreadCrumbs(int startLevel = 0)
        {
            var currentPageModel = _requestModelAccessor.RequestModel.CurrentPageModel;
            var categoryModel = _requestModelAccessor.RequestModel.CurrentCategoryModel;

            if (currentPageModel.Page.ParentPageSystemId == Guid.Empty && categoryModel == null)
            {
                return new BreadCrumbsViewModel();
            }

            var curentChannelModel = _requestModelAccessor.RequestModel.ChannelModel;

            var viewModel = new BreadCrumbsViewModel { ContentLinks = new List<ContentLinkModel>() };
            var parentPages = currentPageModel.GetAncestors();
            foreach (var parentPage in parentPages.Take(parentPages.Count - startLevel).Reverse())
            {
                var pageUrl = _urlService.GetUrl(parentPage.Page);
                viewModel.ContentLinks.Add(CreateContentLink(pageUrl, parentPage.Page.Localizations.CurrentUICulture.Name));
            }

            var buildProductLinkChain = Enumerable.Empty<ContentLinkModel>();
            if (currentPageModel.Page.ParentPageSystemId == Guid.Empty)
            {
                MarketModel market = curentChannelModel.Channel.MarketSystemId.MapTo<MarketModel>();
                if (market != null && market.Market.AssortmentSystemId != Guid.Empty)
                {
                    if (categoryModel != null && market.Market.AssortmentSystemId == categoryModel.Category.AssortmentSystemId)
                    {
                        buildProductLinkChain = BuildProductLinkChain(curentChannelModel, categoryModel, _requestModelAccessor.RequestModel.CurrentProductModel);
                    }
                }
            }

            var urlToPage = _urlService.GetUrl(currentPageModel.Page);
            viewModel.ContentLinks.Add(CreateContentLink(urlToPage, currentPageModel.Page.Localizations.CurrentUICulture.Name));
            viewModel.ContentLinks.AddRange(buildProductLinkChain);
            return viewModel;
        }

        private IEnumerable<ContentLinkModel> BuildProductLinkChain(ChannelModel channelModel, CategoryModel categoryModel, ProductModel productModel)
        {
            var retVal = new List<ContentLinkModel>();

            if (productModel != null)
            {
                if (channelModel.Channel.WebsiteSystemId != null)
                {
                    retVal.Add(CreateContentLink(productModel.GetUrl(channelModel.Channel.WebsiteSystemId.Value, categoryModel.Category,
                            channelModel.SystemId),productModel.GetValue<string>(SystemFieldDefinitionConstants.Name)));
                }
                
            }
            else
            {
                if (channelModel.Channel.WebsiteSystemId != null)
                {
                    retVal.Add(CreateContentLink(
                        categoryModel.Category.GetUrl( channelModel.Channel.SystemId), categoryModel.Category.Localizations.CurrentCulture.Name));
                }
                categoryModel = categoryModel.Category.ParentCategorySystemId.MapTo<CategoryModel>();
            }

            while (categoryModel != null)
            {
                if (channelModel.Channel.WebsiteSystemId != null)
                {
                    retVal.Add(CreateContentLink(categoryModel.Category.GetUrl(channelModel.Channel.SystemId),categoryModel.Category.Localizations.CurrentCulture.Name));
                }
                categoryModel = categoryModel.Category.ParentCategorySystemId.MapTo<CategoryModel>();
            }

            retVal.Reverse();
            return retVal;
        }

        private ContentLinkModel CreateContentLink(string url, string name, bool disabled = false)
        {
            return new ContentLinkModel
            {
                Disabled = disabled,
                Url = url,
                Name = name
            };
        }
    }
}
