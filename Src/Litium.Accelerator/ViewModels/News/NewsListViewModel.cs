using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Constants;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models;
using Litium.Web.Models.Websites;
using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Accelerator.Builders;
using Litium.Accelerator.Extensions;
using Litium.FieldFramework.FieldTypes;

namespace Litium.Accelerator.ViewModels.News
{
    public class NewsListViewModel : IAutoMapperConfiguration, IViewModel
    {
        public string Title { get; set; }
        public string Introduction { get; set; }
        public EditorString Text { get; set; }
        public int NumberOfNewsPerPage { get; set; }
        public IList<LinkModel> Links { get; set; }
        public IList<FileModel> Files { get; set; }
        public IList<NewsViewModel> News { get; set; }
        public PaginationViewModel Pagination { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, NewsListViewModel>()
               .ForMember(x => x.Title, m => m.MapFromField(PageFieldNameConstants.Title))
               .ForMember(x => x.Introduction, m => m.MapFromField(PageFieldNameConstants.Introduction))
               .ForMember(x => x.Text, m => m.MapFrom(newsListPage => newsListPage.GetValue<string>(PageFieldNameConstants.Text)))
               .ForMember(x => x.NumberOfNewsPerPage, m => m.MapFromField(PageFieldNameConstants.NumberOfNewsPerPage))
               .ForMember(x => x.Links, m => m.MapFrom(newsListPage => newsListPage.GetValue<IList<PointerItem>>(PageFieldNameConstants.Links) != null ? newsListPage.GetValue<IList<PointerItem>>(PageFieldNameConstants.Links).OfType<PointerPageItem>().ToList().Select(x => x.MapTo<LinkModel>()).Where(x => x != null) : new List<LinkModel>()))
               .ForMember(x => x.Files, m => m.MapFrom(newsListPage => newsListPage.GetValue<IList<Guid>>(PageFieldNameConstants.Files).Select(x => x.MapTo<FileModel>())));
        }
    }
}
