using Litium.Accelerator.ViewModels.Block;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Blocks;
using Litium.Accelerator.Builders;  // IViewModelBuilder için

namespace Litium.Accelerator.Builders.Block
{
    public class AuthorBlockViewModelBuilder : IViewModelBuilder<AuthorBlockViewModel>
    {
        public virtual AuthorBlockViewModel Build(BlockModel blockModel)
        {
            return blockModel.MapTo<AuthorBlockViewModel>();
        }
    }
} 