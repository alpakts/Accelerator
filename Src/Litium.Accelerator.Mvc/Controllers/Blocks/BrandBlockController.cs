using Litium.Accelerator.Builders.Block;
using Litium.Web.Models.Blocks;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Blocks
{
    public class BrandBlockController : ViewComponent
    {
        private readonly BrandsBlockViewModelBuilder _builder;

        public BrandBlockController(BrandsBlockViewModelBuilder builder)
        {
            _builder = builder;
        }

        public IViewComponentResult Invoke(BlockModel currentBlockModel)
        {
            var model = _builder.Build(currentBlockModel);
            return View("~/Views/Blocks/Brands.cshtml", model);
        }
    }
}