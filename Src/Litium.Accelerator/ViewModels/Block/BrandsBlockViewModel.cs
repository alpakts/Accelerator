using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Blocks;
using JetBrains.Annotations;
using Litium.Accelerator.Builders;
using Litium.Web.Models;
using Litium.Accelerator.Constants;
using Litium.Web.Models.Websites;
using Litium.Accelerator.Extensions;
using Litium.FieldFramework.FieldTypes;

namespace Litium.Accelerator.ViewModels.Block
{
    public class BrandsBlockViewModel : IViewModel, IAutoMapperConfiguration
    {
        public Guid SystemId { get; set; }
        public string Title { get; set; }
        public string LinkText { get; set; }
        public List<LinkModel> Pages { get; set; } = new List<LinkModel>();
        public LinkModel Url { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<BlockModel, BrandsBlockViewModel>()
               .ForMember(x => x.Title, m => m.MapFromField(BlockFieldNameConstants.BlockTitle))
               .ForMember(x => x.LinkText, m => m.MapFromField(BlockFieldNameConstants.LinkText))
               .ForMember(x => x.Pages, m => m.MapFrom(ResolvePages))
               .ForMember(x => x.Url, m => m.MapFrom(brand => brand.GetValue<PointerPageItem>(BlockFieldNameConstants.Link).MapTo<LinkModel>()));
        }

        protected object ResolvePages(BlockModel brandBlock, BrandsBlockViewModel model)
        {
            var rs = new List<LinkModel>();
            var brandPages = brandBlock.GetValue<IList<PointerItem>>(BlockFieldNameConstants.BrandsLinkList)?.OfType<PointerPageItem>().ToList() ?? new List<PointerPageItem>();

            foreach (var brandPage in brandPages)
            {
                var page = brandPage.MapTo<LinkModel>();

                if (page != null)
                {
                    var fileSystemId = brandPage.EntitySystemId.MapTo<PageModel>()?.GetValue<Guid>(PageFieldNameConstants.Image);
                    if (fileSystemId != null && fileSystemId != Guid.Empty)
                    {
                        page.Image = fileSystemId.MapTo<ImageModel>();
                    }

                    rs.Add(page);
                }
            }

            return rs;
        }
    }
}
