using Litium.Accelerator.Builders.Block;
using Litium.Web.Models.Blocks;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Blocks
{
    /// <summary>
    /// Represents the controller for video block.
    /// </summary>
    public class VideoBlockController : ViewComponent
    {
        private readonly VideoBlockViewModelBuilder _builder;

        public VideoBlockController(VideoBlockViewModelBuilder builder)
        {
            _builder = builder;
        }

        public IViewComponentResult Invoke(BlockModel currentBlockModel)
        {
            var model = _builder.Build(currentBlockModel);
            return View("~/Views/Blocks/Video.cshtml", model);
        }
    }
}