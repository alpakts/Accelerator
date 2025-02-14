using System.Threading.Tasks;
using Litium.Accelerator.Builders.Block;
using Litium.Web.Models.Blocks;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Blocks
{
    public class ProductsAndBannerBlockController : ViewComponent
    {
        private readonly ProductsAndBannerBlockViewModelBuilder _builder;

        public ProductsAndBannerBlockController(ProductsAndBannerBlockViewModelBuilder builder)
        {
            _builder = builder;
        }

        public async Task<IViewComponentResult> InvokeAsync(BlockModel currentBlockModel)
        {
            var model = await _builder.BuildAsync(currentBlockModel);
            return View("~/Views/Blocks/ProductsAndBanner.cshtml", model);
        }
    }
}
