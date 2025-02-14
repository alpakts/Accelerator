using Litium.Runtime.AutoMapper;
using AutoMapper;
using Litium.Web.Models.Blocks;
using JetBrains.Annotations;
using Litium.Accelerator.Builders;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;

namespace Litium.Accelerator.ViewModels.Block
{
    public class ProductsAndBannerBlockViewModel : IViewModel, IAutoMapperConfiguration
    {
        public BannersBlockViewModel Banners { get; set; }
        public ProductBlockViewModel Products { get; set; }

        public bool ShowProductToTheRight { get; set; }
        public string ProductLinkText { get; set; }
        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<BlockModel, ProductsAndBannerBlockViewModel>()
               .ForMember(x => x.ShowProductToTheRight, m => m.MapFromField(BlockFieldNameConstants.ShowProductToTheRight))
               .ForMember(x => x.ProductLinkText, m => m.MapFromField(BlockFieldNameConstants.ProductLinkText));
        }
    }
}
