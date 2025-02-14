using Litium.Accelerator.ViewModels.Block;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Blocks;

namespace Litium.Accelerator.Builders.Block
{
    public class BannersBlockViewModelBuilder : IViewModelBuilder<BannersBlockViewModel>
    {
        /// <summary>
        /// Build the banner block view model
        /// </summary>
        /// <param name="blockModel">The current banner block banner</param>
        /// <returns>Return the banner block view model</returns>
        public virtual BannersBlockViewModel Build(BlockModel blockModel)
        {
            return blockModel.MapTo<BannersBlockViewModel>();
        }
    }
}
