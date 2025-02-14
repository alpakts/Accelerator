using Litium.Accelerator.ViewModels.Block;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Blocks;

namespace Litium.Accelerator.Builders.Block
{
    public class SliderBlockViewModelBuilder : IViewModelBuilder<SliderBlockViewModel>
    {
        /// <summary>
        /// Build the slideshow block view model
        /// </summary>
        /// <param name="blockModel">The current block</param>
        /// <returns>Return the slideshow block view model</returns>
        public virtual SliderBlockViewModel Build(BlockModel blockModel)
        {
            return blockModel.MapTo<SliderBlockViewModel>();
        }
    }
}
