using System.Threading.Tasks;
using Litium.Accelerator.Builders.Block;
using Litium.Web.Models.Blocks;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Blocks
{
    /// <summary>
    /// Represents the controller for product block.
    /// </summary>
    /// <remarks>Have to name it to have 'Block' in the name, otherwise we will have this error since both Product page
    /// and Product block have the same controller name.
    /// Multiple types were found that match the controller named 'Product'.</remarks>
    public class ProductBlockController : ViewComponent
    {
        private readonly ProductBlockViewModelBuilder _builder;

        public ProductBlockController(ProductBlockViewModelBuilder builder)
        {
            _builder = builder;
        }

        public async Task<IViewComponentResult> InvokeAsync(BlockModel currentBlockModel)
        {
            var model = await _builder.BuildAsync(currentBlockModel);
            return View("~/Views/Blocks/Product.cshtml", model);
        }
    }
}
