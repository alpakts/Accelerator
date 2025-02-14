using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Search;
using Litium.Accelerator.ViewModels.Brand;
using Litium.Accelerator.ViewModels.Framework;
using Litium.Runtime.AutoMapper;
using Litium.Web;
using Litium.Web.Models.Websites;
using Litium.Websites;

namespace Litium.Accelerator.Builders.Brand
{
    public class BrandListViewModelBuilder : IViewModelBuilder<BrandListViewModel>
    {
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly PageService _pageService;
        private readonly UrlService _urlService;
        private readonly ProductSearchService _productSearchService;

        public BrandListViewModelBuilder(
            RequestModelAccessor requestModelAccessor,
            PageService pageService,
            UrlService urlService,
            ProductSearchService productSearchService)
        {
            _pageService = pageService;
            _urlService = urlService;
            _requestModelAccessor = requestModelAccessor;
            _productSearchService = productSearchService;
        }

        public virtual async Task<BrandListViewModel> BuildAsync(PageModel pageModel)
        {
            var model = pageModel.MapTo<BrandListViewModel>();
            model.Nodes = await GetBrandNodesAsync(model.MaxColumns);
            return model;
        }

        private async Task<BrandListViewModel.BrandNode> GetBrandNodesAsync(int maxColumns)
        {
            var brandNodes = new BrandListViewModel.BrandNode();

            var allBrandPages = await GetNavigationModelAsync();
            if (!allBrandPages.Any())
            {
                return brandNodes;
            }
            
            var average = Convert.ToInt32(Math.Round((decimal)allBrandPages.Count() / maxColumns, MidpointRounding.AwayFromZero));
            var rowIndex = 0;

            var node = new BrandListViewModel.BrandNode();
            brandNodes.Add(node);
            BrandListViewModel.BrandNode letterGroup = null;
            foreach (var brandPage in allBrandPages)
            {
                if (brandPage.Name is object)
                {
                    var brandContent = new ContentLinkModel()
                    {
                        Name = brandPage.Name,
                        Url = brandPage.Url,
                        ExtraInfo = brandPage.ExtraInfo
                    };

                    var firstLetter = brandContent.Name.Substring(0, 1);
                    if (letterGroup == null || !firstLetter.Equals(letterGroup.Value.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        letterGroup = new BrandListViewModel.BrandNode(new ContentLinkModel { Name = firstLetter });
                        node.Add(letterGroup);

                        if (rowIndex > average)
                        {
                            rowIndex = 0;
                            node = new BrandListViewModel.BrandNode();
                            brandNodes.Add(node);
                        }
                    }

                    letterGroup.Add(new BrandListViewModel.BrandNode(brandContent));
                    rowIndex++;
                }
            }

            if (node.Count == 0)
            {
                brandNodes.Remove(node);
            }

            return brandNodes;
        }

        private async Task<IEnumerable<ContentLinkModel>> GetNavigationModelAsync()
        {
            var searchQuery = new SearchQuery()
            {
                PageSize = 500,
                PageNumber = 1
            };
            var tagTerm = (await _productSearchService.GetTagTermsAsync(searchQuery, new[] { BrandListViewModel.TagName })).FirstOrDefault();

            var currentPageId = _requestModelAccessor.RequestModel.CurrentPageModel.SystemId;
            var channelId = _requestModelAccessor.RequestModel?.ChannelModel?.SystemId ?? Guid.Empty;

            return _pageService.GetChildPages(currentPageId)
                               .Where(x => x.IsActive(channelId))
                               .Select(x => new ContentLinkModel
                               {
                                   Url = _urlService.GetUrl(x),
                                   Name = x.Localizations.CurrentUICulture.Name,
                                   IsSelected = x.SystemId == currentPageId,
                                   ExtraInfo = tagTerm == null ? "0" : tagTerm.TermCounts.Where(z => z.Term.Equals(x.Localizations.CurrentCulture.Name, StringComparison.OrdinalIgnoreCase)).Select(z => z.Count.ToString(CultureInfo.InvariantCulture)).Concat(new[] { "0" }).FirstOrDefault()
                               })
                               .OrderBy(x => x.Name);
        }
    }
}
