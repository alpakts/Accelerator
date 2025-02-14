using Litium.Accelerator.Builders.Block;
using Litium.Web.Models.Blocks;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Blocks
{
    public class BannerBlockController : ViewComponent
    {
        private readonly BannersBlockViewModelBuilder _builder;

        public BannerBlockController(BannersBlockViewModelBuilder builder)
        {
            _builder = builder;
        }

        public IViewComponentResult Invoke(BlockModel currentBlockModel)
        {
            var model = _builder.Build(currentBlockModel);
            return View("~/Views/Blocks/Banners.cshtml", model);
        }
    }
}