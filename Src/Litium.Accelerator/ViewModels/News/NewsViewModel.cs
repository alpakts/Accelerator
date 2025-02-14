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

namespace Litium.Accelerator.ViewModels.News
{
    public class NewsViewModel : IAutoMapperConfiguration, IViewModel
    {
        public Guid SystemId { get; set; }
        public string Title { get; set; }
        public DateTime NewsDate { get; set; }
        public string Introduction { get; set; }
        public EditorString Text { get; set; }
        public IList<LinkModel> Links { get; set; }
        public IList<FileModel> Files { get; set; }
        public ImageModel Image { get; set; }
        public Dictionary<string, List<BlockModel>> Blocks { get; set; }
        public string Url { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, NewsViewModel>()
               .ForMember(x => x.Title, m => m.MapFromField(PageFieldNameConstants.Title))
               .ForMember(x => x.Introduction, m => m.MapFromField(PageFieldNameConstants.Introduction))
               .ForMember(x => x.Text, m => m.MapFrom(newsPage => newsPage.GetValue<string>(PageFieldNameConstants.Text)))
               .ForMember(x => x.NewsDate, m => m.MapFrom(newspage => newspage.GetValue<DateTimeOffset>(PageFieldNameConstants.NewsDate)))
               .ForMember(x => x.Links, m => m.MapFrom(newsPage => newsPage.GetValue<IList<PointerItem>>(PageFieldNameConstants.Links) != null ? newsPage.GetValue<IList<PointerItem>>(PageFieldNameConstants.Links).OfType<PointerPageItem>().ToList().Select(x => x.MapTo<LinkModel>()).Where(x => x != null) : new List<LinkModel>()))
               .ForMember(x => x.Files, m => m.MapFrom(newsPage => newsPage.GetValue<IList<Guid>>(PageFieldNameConstants.Files).Select(x => x.MapTo<FileModel>())))
               .ForMember(x => x.Image, m => m.MapFrom(newsPage => newsPage.GetValue<Guid>(PageFieldNameConstants.Image).MapTo<ImageModel>()));
        }
    }
}
