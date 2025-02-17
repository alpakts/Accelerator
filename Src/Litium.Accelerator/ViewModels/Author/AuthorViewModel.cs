using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Constants;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models;
using Litium.Web.Models.Blocks;
using Litium.Web.Models.Websites;
using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Accelerator.Builders;
using Litium.Accelerator.Extensions;
using Litium.FieldFramework.FieldTypes;

namespace Litium.Accelerator.ViewModels.Author
{
    public class AuthorViewModel : IAutoMapperConfiguration, IViewModel
    {
        public Dictionary<string, List<BlockModel>> Blocks { get; set; }
        public string Introduction { get; set; }
        public string Title { get; set; }
        public EditorString Text { get; set; }
        public ImageModel Image { get; set; }
        public IList<LinkModel> Links { get; set; }
        public IList<FileModel> Files { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, AuthorViewModel>()
               .ForMember(x => x.Title, m => m.MapFromField(PageFieldNameConstants.Title))
               .ForMember(x => x.Introduction, m => m.MapFromField(PageFieldNameConstants.Introduction))
               .ForMember(x => x.Text, m => m.MapFrom(AuthorPage => AuthorPage.GetValue<string>(PageFieldNameConstants.Text)))
               .ForMember(x => x.Image, m => m.MapFrom<ImageModelResolver>())
               .ForMember(x => x.Links, m => m.MapFrom(AuthorPage => AuthorPage.GetValue<IList<PointerItem>>(PageFieldNameConstants.Links) != null ? AuthorPage.GetValue<IList<PointerItem>>(PageFieldNameConstants.Links).OfType<PointerPageItem>().ToList().Select(x => x.MapTo<LinkModel>()).Where(x => x != null) : new List<LinkModel>()))
               .ForMember(x => x.Files, m => m.MapFrom(AuthorPage => AuthorPage.GetValue<IList<Guid>>(PageFieldNameConstants.Files).Select(x => x.MapTo<FileModel>())));
        }

        [UsedImplicitly]
        protected class ImageModelResolver : IValueResolver<PageModel, AuthorViewModel, ImageModel>
        {
            public ImageModel Resolve(PageModel source, AuthorViewModel AuthorViewModel, ImageModel destMember, ResolutionContext context)
            {
                var imageModel = source.GetValue<Guid>(PageFieldNameConstants.Image).MapTo<ImageModel>();
                if (imageModel is not null)
                {
                    var altImage = source.GetValue<string>(PageFieldNameConstants.AlternativeImageDescription);
                    if (!string.IsNullOrEmpty(altImage))
                    {
                        imageModel.Alt = altImage;
                    }

                    return imageModel;
                }

                return null;
            }
        }
    }
}

