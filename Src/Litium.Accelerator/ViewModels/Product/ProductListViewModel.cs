using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Constants;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models;
using Litium.Web.Models.Websites;
using System;
using System.Collections.Generic;
using Litium.Accelerator.Builders;
using Litium.Accelerator.Extensions;

namespace Litium.Accelerator.ViewModels.Product
{
    public class ProductListViewModel : IAutoMapperConfiguration, IViewModel
    {
        /// <summary>
        /// The title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The image description
        /// </summary>
        public string AlternativeImageDescription { get; set; }

        /// <summary>
        /// The image
        /// </summary>
        public ImageModel Image { get; set; }

        /// <summary>
        /// List product
        /// </summary>
        public List<ProductItemViewModel> Products { get; set; }

        /// <summary>
        /// The pagination
        /// </summary>
        public PaginationViewModel Pagination { get; set; }


        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, ProductListViewModel>()
               .ForMember(x => x.Title, m => m.MapFromField(PageFieldNameConstants.Title))
               .ForMember(x => x.AlternativeImageDescription, m => m.MapFromField(PageFieldNameConstants.AlternativeImageDescription))
               .ForMember(x => x.Image, m => m.MapFrom(newsPage => newsPage.GetValue<Guid>(PageFieldNameConstants.Image).MapTo<ImageModel>()));
        }
    }
}
