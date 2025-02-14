using System.Threading.Tasks;
using Litium.Accelerator.ViewModels.Block;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Blocks;

namespace Litium.Accelerator.Builders.Block
{
    public class ProductsAndBannerBlockViewModelBuilder : IViewModelBuilder<ProductsAndBannerBlockViewModel>
    {
        private readonly BannersBlockViewModelBuilder _bannersViewModelBuilder;
        private readonly ProductBlockViewModelBuilder _productViewModelBuilder;

        public ProductsAndBannerBlockViewModelBuilder(BannersBlockViewModelBuilder bannersViewModelBuilder, ProductBlockViewModelBuilder productViewModelBuilder)
        {
            _bannersViewModelBuilder = bannersViewModelBuilder;
            _productViewModelBuilder = productViewModelBuilder;
        }

        /// <summary>
        /// Build the mixed block view model
        /// </summary>
        /// <param name="blockModel">The current mixed block</param>
        /// <returns>Return the mixed block view model</returns>
        public virtual async Task<ProductsAndBannerBlockViewModel> BuildAsync(BlockModel blockModel)
        {
            var sectionBanners = _bannersViewModelBuilder.Build(blockModel);
            var sectionProducts = await _productViewModelBuilder.BuildAsync(blockModel);

            var mixedBlockViewModel = blockModel.MapTo<ProductsAndBannerBlockViewModel>();

            mixedBlockViewModel.Products = sectionProducts;
            mixedBlockViewModel.Products.FooterLinkText = mixedBlockViewModel.ProductLinkText;

            mixedBlockViewModel.Banners = sectionBanners;
            return mixedBlockViewModel;
        }
    }
}
