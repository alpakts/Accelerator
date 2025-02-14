using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Builders;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.ViewModels.Product;
using Litium.FieldFramework;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models;
using Litium.Web.Models.Blocks;
using Litium.Web.Models.Websites;

namespace Litium.Accelerator.ViewModels.Brand
{
    public class BrandViewModel : IAutoMapperConfiguration, IViewModel
    {
        public bool ContainsFilter { get; set; }
        public bool ShowFilterHeader { get; set; }
        public bool ShowRegularHeader { get; set; }

        public EditorString Text { get; set; }
        public string Title { get; set; }
        public ImageModel Image { get; set; }
        public Dictionary<string, List<BlockModel>> Blocks { get; set; }

        public IEnumerable<ProductItemViewModel> Products { get; set; } = Enumerable.Empty<ProductItemViewModel>();
        public PaginationViewModel Pagination { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, BrandViewModel>()
               .ForMember(x => x.Title, m => m.MapFromField(PageFieldNameConstants.Title))
               .ForMember(x => x.Text, m => m.MapFrom(brandPage => brandPage.GetValue<string>(PageFieldNameConstants.Text)))
               .ForMember(x => x.Image, m => m.MapFrom(brandPage => brandPage.GetValue<Guid>(PageFieldNameConstants.Image).MapTo<ImageModel>()))
               .AfterMap((source, destination) =>
               {
                   if (destination.Image is not null && string.IsNullOrEmpty(destination.Image.Alt))
                   {
                       destination.Image.Alt = !string.IsNullOrEmpty(destination.Title)
                           ? destination.Title
                           : source.GetValue<string>(SystemFieldDefinitionConstants.Name);
                   }
               });
        }
    }
}
