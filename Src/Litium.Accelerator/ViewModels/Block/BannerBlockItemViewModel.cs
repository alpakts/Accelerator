using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Constants;
using Litium.FieldFramework;
using Litium.FieldFramework.FieldTypes;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Web;
using Litium.Web.Models;
using Litium.Web.Routing;
using System;
using System.Globalization;

namespace Litium.Accelerator.ViewModels.Block
{
    public class BannerBlockItemViewModel : IAutoMapperConfiguration
    {
        public ImageModel Image { get; set; }
        public string ActionText { get; set; }
        public string LinkText { get; set; }
        public string LinkUrl { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<MultiFieldItem, BannerBlockItemViewModel>()
               .ForMember(x => x.LinkText, m => m.MapFrom(c => c.Fields.GetValue<string>(BlockFieldNameConstants.LinkText, CultureInfo.CurrentUICulture)))
               .ForMember(x => x.ActionText, m => m.MapFrom(c => c.Fields.GetValue<string>(BlockFieldNameConstants.ActionText, CultureInfo.CurrentUICulture)))
               .ForMember(x => x.Image, m => m.MapFrom(c => c.Fields.GetValue<Guid>(BlockFieldNameConstants.BlockImagePointer, CultureInfo.CurrentUICulture).MapTo<ImageModel>()))
               .ForMember(x => x.LinkUrl, m => m.MapFrom<LinkUrlResolver>());
        }

        [UsedImplicitly]
        private class LinkUrlResolver : IValueResolver<MultiFieldItem, BannerBlockItemViewModel, string>
        {
            private readonly RouteRequestLookupInfoAccessor _routeRequestLookupInfoAccessor;
            private readonly UrlService _urlService;

            public LinkUrlResolver(RouteRequestLookupInfoAccessor routeRequestLookupInfoAccessor,
                UrlService urlService)
            {
                _routeRequestLookupInfoAccessor = routeRequestLookupInfoAccessor;
                _urlService = urlService;
            }

            public string Resolve(MultiFieldItem source, BannerBlockItemViewModel destination, string destMember, ResolutionContext context)
            {
                var channelRequestLookupInfo = _routeRequestLookupInfoAccessor.RouteRequestLookupInfo?.Channel;
                if (channelRequestLookupInfo == null)
                {
                    return "";
                }
                var category = source.Fields.GetValue<Guid?>(BlockFieldNameConstants.BannerLinkToCategory)?.MapTo<Category>();
                if (category != null)
                {
                    return category.GetUrl(channelRequestLookupInfo.SystemId);
                }

                var pointerPageItem = source.Fields.GetValue<PointerPageItem>(BlockFieldNameConstants.BannerLinkToPage);
                var page = pointerPageItem?.EntitySystemId.MapTo<Websites.Page>();
                if (page != null)
                {
                    return _urlService.GetUrl(page, new PageUrlArgs(pointerPageItem.ChannelSystemId == Guid.Empty ? channelRequestLookupInfo.SystemId : pointerPageItem.ChannelSystemId));
                }
                var baseProduct = source.Fields.GetValue<Guid?>(BlockFieldNameConstants.BannerLinkToProduct)?.MapTo<BaseProduct>();
                if (baseProduct != null)
                {
                    return baseProduct.GetUrl(channelRequestLookupInfo.WebsiteSystemId.GetValueOrDefault(), channelSystemId: channelRequestLookupInfo.SystemId);
                }
                return "";
            }
        }
    }
}
