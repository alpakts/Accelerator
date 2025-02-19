using System;
using AutoMapper;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Blocks;
using JetBrains.Annotations;
using Litium.Accelerator.Constants;
using Litium.Web.Models;
using Litium.FieldFramework.FieldTypes;
using System.Globalization;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Block
{
    public class AuthorBlockViewModel : IViewModel, IAutoMapperConfiguration
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public ImageModel Image { get; set; }
        public string LinkUrl { get; set; }
        public string LinkText { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<BlockModel, AuthorBlockViewModel>()
               .ForMember(x => x.Title, m => m.MapFromField(BlockFieldNameConstants.BlockTitle))
               .ForMember(x => x.Text, m => m.MapFromField(BlockFieldNameConstants.BlockText))
               .ForMember(x => x.Image, m => m.MapFrom(new ImageResolver()))
               .ForMember(x => x.LinkText, m => m.MapFromField(BlockFieldNameConstants.LinkText))
               .ForMember(x => x.LinkUrl, m => m.MapFrom(new LinkUrlResolver()));
        }

        private class ImageResolver : IValueResolver<BlockModel, AuthorBlockViewModel, ImageModel>
        {
            public ImageModel Resolve(BlockModel source, AuthorBlockViewModel destination, ImageModel destMember, ResolutionContext context)
            {
                var imageId = source.GetValue<Guid>(BlockFieldNameConstants.BlockImagePointer);
                return imageId == Guid.Empty ? null : imageId.MapTo<ImageModel>();
            }
        }

        private class LinkUrlResolver : IValueResolver<BlockModel, AuthorBlockViewModel, string>
        {
            public string Resolve(BlockModel source, AuthorBlockViewModel destination, string destMember, ResolutionContext context)
            {
                var link = source.GetValue<PointerPageItem>(BlockFieldNameConstants.Link);
                if (link == null) return string.Empty;
                var linkModel = link.MapTo<LinkModel>();
                return linkModel?.Href ?? string.Empty;
            }
        }
    }
} 