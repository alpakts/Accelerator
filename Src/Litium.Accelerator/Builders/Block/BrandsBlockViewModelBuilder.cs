using Litium.Accelerator.ViewModels.Block;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Blocks;

namespace Litium.Accelerator.Builders.Block
{
    public class BrandsBlockViewModelBuilder : IViewModelBuilder<BrandsBlockViewModel>
    {
        /// <summary>
        /// Build the brand block view model
        /// </summary>
        /// <param name="blockModel">The current brand block</param>
        /// <returns>Return the brand block view model</returns>
        public virtual BrandsBlockViewModel Build(BlockModel blockModel)
        {
            return blockModel.MapTo<BrandsBlockViewModel>();
        }
    }
}
