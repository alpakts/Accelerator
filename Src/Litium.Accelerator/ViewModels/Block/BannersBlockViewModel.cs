using System;
using System.Linq;
using System.Collections.Generic;
using AutoMapper;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Blocks;
using Litium.FieldFramework;
using Litium.Blocks;
using JetBrains.Annotations;
using Litium.Accelerator.Builders;
using Litium.Accelerator.Constants;
using Litium.Web.Models;
using Litium.FieldFramework.FieldTypes;
using Litium.Web.Routing;
using Litium.Web;

namespace Litium.Accelerator.ViewModels.Block
{
    public class BannersBlockViewModel : IViewModel, IAutoMapperConfiguration
    {
        public Guid SystemId { get; set; }

        public List<BannerBlockItemViewModel> Banners { get; set; } = new List<BannerBlockItemViewModel>();

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<BlockModel, BannersBlockViewModel>()
               .ForMember(x => x.Banners, m => m.MapFrom<BannersResolver>());
        }

        private class BannersResolver : IValueResolver<BlockModel, BannersBlockViewModel, List<BannerBlockItemViewModel>>
        {
            private readonly FieldTemplateService _fieldTemplateService;
            private readonly RouteRequestLookupInfoAccessor _routeRequestLookupInfoAccessor;
            private readonly UrlService _urlService;

            public BannersResolver(FieldTemplateService fieldTemplateService,
                RouteRequestLookupInfoAccessor routeRequestLookupInfoAccessor,
                UrlService urlService)
            {
                _fieldTemplateService = fieldTemplateService;
                _routeRequestLookupInfoAccessor = routeRequestLookupInfoAccessor;
                _urlService = urlService;
            }

            public List<BannerBlockItemViewModel> Resolve(BlockModel block, BannersBlockViewModel bannersViewModel, List<BannerBlockItemViewModel> destMember, ResolutionContext context)
            {
                var result = new List<BannerBlockItemViewModel>();
                var blockTemplate = _fieldTemplateService.Get<FieldTemplateBase>(block.FieldTemplateSystemId);
                if (blockTemplate.FieldGroups.Any(x => x.Id == "Banners"))
                {
                    var banners = block.GetValue<IList<MultiFieldItem>>(BlockFieldNameConstants.Banners);
                    if (banners != null)
                    {
                        result.AddRange(banners.Select(c => c.MapTo<BannerBlockItemViewModel>()));
                    }
                }
                else if (blockTemplate.FieldGroups.Any(x => x.Id == "Banner"))
                {
                    var banner = new BannerBlockItemViewModel()
                    {
                        LinkText = block.GetValue<string>(BlockFieldNameConstants.LinkText),
                        ActionText = block.GetValue<string>(BlockFieldNameConstants.ActionText),
                        Image = block.GetValue<Guid>(BlockFieldNameConstants.BlockImagePointer).MapTo<ImageModel>(),
                        LinkUrl = GetBannerUrl(block)
                    };
                    result.Add(banner);
                }

                return result;
            }

            private string GetBannerUrl(BlockModel blockModel)
            {
                var channelRequestLookupInfo = _routeRequestLookupInfoAccessor.RouteRequestLookupInfo?.Channel;
                if (channelRequestLookupInfo == null)
                {
                    return "";
                }
                var category = blockModel.Fields.GetValue<Guid?>(BlockFieldNameConstants.BannerLinkToCategory)?.MapTo<Products.Category>();
                if (category != null)
                {
                    return category.GetUrl(channelRequestLookupInfo.SystemId);
                }

                var pointerPageItem = blockModel.Fields.GetValue<PointerPageItem>(BlockFieldNameConstants.BannerLinkToPage);
                var page = pointerPageItem?.EntitySystemId.MapTo<Websites.Page>();
                if (page != null)
                {
                    return _urlService.GetUrl(page, new PageUrlArgs(pointerPageItem.ChannelSystemId == Guid.Empty ? channelRequestLookupInfo.SystemId : pointerPageItem.ChannelSystemId));
                }
                var baseProduct = blockModel.Fields.GetValue<Guid?>(BlockFieldNameConstants.BannerLinkToProduct)?.MapTo<Products.BaseProduct>();
                if (baseProduct != null)
                {
                    return baseProduct.GetUrl(channelRequestLookupInfo.WebsiteSystemId.GetValueOrDefault(),
                        channelSystemId: channelRequestLookupInfo.SystemId);
                }
                return "";
            }
        }
    }
}
