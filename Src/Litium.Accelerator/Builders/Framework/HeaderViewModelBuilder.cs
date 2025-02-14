using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Accelerator.Caching;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Routing;
using Litium.Accelerator.ViewModels.Framework;
using Litium.FieldFramework.FieldTypes;
using Litium.Runtime.AutoMapper;
using Litium.Web;
using Litium.Web.Models;
using Litium.Web.Routing;
using Litium.Websites;

namespace Litium.Accelerator.Builders.Framework
{
    public class HeaderViewModelBuilder<TViewModel> : IViewModelBuilder<TViewModel>
        where TViewModel : HeaderViewModel
    {
        private readonly UrlService _urlService;
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly PageByFieldTemplateCache<LoginPageByFieldTemplateCache> _pageByFieldType;
        private readonly PageService _pageService;
        private readonly RouteRequestLookupInfoAccessor _routeRequestLookupInfoAccessor;

        public HeaderViewModelBuilder(
            UrlService urlService,
            RequestModelAccessor requestModelAccessor,
            PageByFieldTemplateCache<LoginPageByFieldTemplateCache> pageByFieldType,
            PageService pageService,
            RouteRequestLookupInfoAccessor routeRequestLookupInfoAccessor)
        {
            _urlService = urlService;
            _requestModelAccessor = requestModelAccessor;
            _pageByFieldType = pageByFieldType;
            _pageService = pageService;
            _routeRequestLookupInfoAccessor = routeRequestLookupInfoAccessor;
        }

        public HeaderViewModel Build()
        {
            var website = _requestModelAccessor.RequestModel.WebsiteModel;
            var myPage = website.GetValue<PointerPageItem>(AcceleratorWebsiteFieldNameConstants.MyPagesPage)?.MapTo<LinkModel>();
            LinkModel loginPage = null;
            _pageByFieldType.TryFindPage(login =>
            {
                if (login == null)
                {
                    return false;
                }

                loginPage = CreateLoginLink(login.MapTo<LinkModel>());
                return loginPage != null;
            }, true);

            var topLinkList = website.GetValue<IList<PointerItem>>(AcceleratorWebsiteFieldNameConstants.AdditionalHeaderLinks)?.OfType<PointerPageItem>().ToList().Select(x => x.MapTo<LinkModel>()).Where(x => x != null).ToList();
            myPage = myPage?.AccessibleByUser == true ? myPage : null;

            var startPage = _pageService.GetChildPages(Guid.Empty, website.SystemId).FirstOrDefault();
            var startPageUrl = _urlService.GetUrl(startPage, new PageUrlArgs(_requestModelAccessor.RequestModel.ChannelModel.SystemId));
            var logo = website.GetValue<Guid?>(AcceleratorWebsiteFieldNameConstants.LogotypeMain)?.MapTo<ImageModel>();
            if (logo is not null)
            {
                if (string.IsNullOrEmpty(logo.Alt))
                {
                    logo.Alt = website.Texts.GetValue("site.title");
                }
            }
            return new HeaderViewModel
            {
                Logo = logo,
                TopLinkList = topLinkList ?? new List<LinkModel>(),
                LoginPage = loginPage ?? new LinkModel(),
                MyPage = myPage ?? new LinkModel(),
                StartPageUrl = string.IsNullOrWhiteSpace(startPageUrl) ? "/" : startPageUrl,
                IsBigHeader = website.GetValue<string>(AcceleratorWebsiteFieldNameConstants.HeaderLayout) == HeaderLayoutConstants.TwoRows,
                HeaderName = website.GetValue<string>(AcceleratorWebsiteFieldNameConstants.HeaderLayout),
                QuickSearch = new QuickSearchViewModel
                {
                    IsBigHeader = website.GetValue<string>(AcceleratorWebsiteFieldNameConstants.HeaderLayout) == HeaderLayoutConstants.TwoRows,
                    SearchTerm = _requestModelAccessor.RequestModel.SearchQuery.Text
                }
            };
        }

        private LinkModel CreateLoginLink(LinkModel loginPage)
        {
            if (loginPage == null || !loginPage.AccessibleByUser)
            {
                return null;
            }

            if (loginPage.Href != null)
            {
                loginPage.Href += string.Concat(loginPage.Href.Contains("?") ? "&" : "?", "RedirectUrl=", System.Web.HttpUtility.UrlEncode(_routeRequestLookupInfoAccessor.RouteRequestLookupInfo.RawUrl));
            }
            return loginPage;
        }
    }
}
