using AutoMapper;
using JetBrains.Annotations;
using Litium.FieldFramework;
using Litium.Media;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models;
using Litium.Web.Models.Products;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Litium.Accelerator.Extensions;
using Litium.Products;

namespace Litium.Accelerator.ViewModels.Search
{
    public class ProductSearchResult : SearchResultItem, IAutoMapperConfiguration
    {
        public ProductModel Item { get; set; }

        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ProductSearchResult, SearchItem>()
                .ForMember(x => x.Name, m => m.MapFrom<ProductNameResolver>())
               .ForMember(x => x.HasImage, m => m.MapFrom((c, _) =>
               {
                   var firstImg = c.Item.GetValue<IList<Guid>>(SystemFieldDefinitionConstants.Images)?.FirstOrDefault();
                   return firstImg.HasValue;
               }))
               .ForMember(x => x.ImageSource, m => m.MapFrom<ImageSourceResolver>());
        }

        [UsedImplicitly]
        private class ImageSourceResolver : IValueResolver<ProductSearchResult, SearchItem, string>
        {
            private readonly FileService _fileService;

            public ImageSourceResolver(FileService fileService)
            {
                _fileService = fileService;
            }

            public string Resolve(ProductSearchResult source, SearchItem destination, string destMember, ResolutionContext context)
            {
                var firstImg = source.Item.GetValue<IList<Guid>>(SystemFieldDefinitionConstants.Images)?.FirstOrDefault();
                return firstImg.HasValue ? _fileService.Get(firstImg.Value)?.MapTo<ImageModel>().GetUrlToImage(Size.Empty, new Size(30, -1)).Url : null;
            }
        }

        [UsedImplicitly]
        private class ProductNameResolver : IValueResolver<ProductSearchResult, SearchItem, string>
        {
            private readonly FieldDefinitionService _fieldDefinitionService;

            public ProductNameResolver(FieldDefinitionService fieldDefinitionService)
            {
                _fieldDefinitionService = fieldDefinitionService;
            }

            public string Resolve(ProductSearchResult source, SearchItem destination, string destMember, ResolutionContext context)
            {
                var brandName = _fieldDefinitionService.Get<ProductArea>("Brand").GetTranslation(source.Item.GetValue<string>("Brand"), CultureInfo.CurrentCulture);
                return $"{brandName} {source.Name}".Trim();
            }
        }
    }
}
