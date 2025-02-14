using System;
using System.Collections.Generic;
using AutoMapper;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Blocks;
using JetBrains.Annotations;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.FieldFramework.FieldTypes;
using Litium.Web.Models;

namespace Litium.Accelerator.ViewModels.Block
{
    public class ProductBlockViewModelData : IAutoMapperConfiguration
    {
        public Guid SystemId { get; set; }
        public string Title { get; set; }
        public Guid CategorySystemId { get; set; }
        public string LinkText { get; set; }
        public Guid LinkToCategorySystemId { get; set; }
        public LinkModel LinkToPage { get; set; }
        public int NumberOfProducts { get; set; }
        public Guid ProductListSystemId { get; set; }
        public string ProductSorting { get; set; }
        public IList<Guid> ProductSystemIds { get; set; }
        public BlockProductsType SectionProductsType { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<BlockModel, ProductBlockViewModelData>()
               .ForMember(x => x.Title, m => m.MapFromField(BlockFieldNameConstants.BlockTitle))
               .ForMember(x => x.CategorySystemId, m => m.MapFrom(product => product.GetValue<Guid?>(BlockFieldNameConstants.CategoryLink).GetValueOrDefault()))
               .ForMember(x => x.LinkText, m => m.MapFromField(BlockFieldNameConstants.LinkText))
               .ForMember(x => x.LinkToCategorySystemId, m => m.MapFrom(product => product.GetValue<Guid?>(BlockFieldNameConstants.LinkToCategory).GetValueOrDefault()))
               .ForMember(x => x.LinkToPage, m => m.MapFrom(product => product.GetValue<PointerPageItem>(BlockFieldNameConstants.LinkToPage).MapTo<LinkModel>() ?? new LinkModel()))
               .ForMember(x => x.NumberOfProducts, m => m.MapFrom(product => product.GetValue<int?>(BlockFieldNameConstants.NumberOfProducts) ?? 4))
               .ForMember(x => x.ProductListSystemId, m => m.MapFrom(product => product.GetValue<Guid?>(BlockFieldNameConstants.ProductListLink).GetValueOrDefault()))
               .ForMember(x => x.ProductSystemIds, m => m.MapFrom(product => product.GetValue<IList<Guid>>(BlockFieldNameConstants.ProductsLinkList) ?? new List<Guid>()))
               .ForMember(c => c.ProductSorting, m => m.MapFromField(BlockFieldNameConstants.ProductSorting))
               .AfterMap((block, productBlockModelData) =>
               {
                   if (productBlockModelData.CategorySystemId != Guid.Empty)
                   {
                       productBlockModelData.SectionProductsType = BlockProductsType.Category;
                   }
                   else if (productBlockModelData.ProductListSystemId != Guid.Empty)
                   {
                       productBlockModelData.SectionProductsType = BlockProductsType.ProductList;
                   }
                   else if (productBlockModelData.ProductSystemIds.Count > 0)
                   {
                       productBlockModelData.SectionProductsType = BlockProductsType.Products;
                   }
               });
        }
    }

    public enum BlockProductsType
    {
        Category,
        ProductList,
        Products
    }
}
