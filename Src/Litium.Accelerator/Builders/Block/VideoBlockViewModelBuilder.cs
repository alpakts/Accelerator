using Litium.Accelerator.ViewModels.Block;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Blocks;

namespace Litium.Accelerator.Builders.Block
{
    public class VideoBlockViewModelBuilder : IViewModelBuilder<VideoBlockViewModel>
    {
        /// <summary>
        /// Build the video block view model
        /// </summary>
        /// <param name="blockModel">The current block</param>
        /// <returns>Return the video block view model</returns>
        public virtual VideoBlockViewModel Build(BlockModel blockModel)
        {
            return blockModel.MapTo<VideoBlockViewModel>();
        }
    }
}
