using Litium.Accelerator.Builders.Block;
using Litium.Web.Models.Blocks;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Blocks
{
    /// <summary>
    /// Represents the controller for slider block.
    /// </summary>
    public class SliderBlockController : ViewComponent
    {
        private readonly SliderBlockViewModelBuilder _builder;

        public SliderBlockController(SliderBlockViewModelBuilder builder)
        {
            _builder = builder;
        }

        public IViewComponentResult Invoke(BlockModel currentBlockModel)
        {
            var model = _builder.Build(currentBlockModel);
            return View("~/Views/Blocks/Slider.cshtml", model);
        }
    }
}