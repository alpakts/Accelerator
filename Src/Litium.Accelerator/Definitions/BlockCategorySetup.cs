using System.Collections.Generic;
using Litium.Blocks;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Definitions
{
    [Service(ServiceType = typeof(BlockCategorySetup))]
    public abstract class BlockCategorySetup
    {
        public abstract IEnumerable<Category> GetCategories();
    }
}
