using Litium.Web.Models.Websites;
using Litium.Accelerator.Builders.Brand;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Litium.Accelerator.Mvc.Controllers.Brand
{
    public class BrandController : ControllerBase
    {
        private readonly BrandViewModelBuilder _brandViewModelBuilder;
        private readonly BrandListViewModelBuilder _brandListViewModelBuilder;
        
        public BrandController(BrandViewModelBuilder brandViewModelBuilder, BrandListViewModelBuilder brandListViewModelBuilder)
        {
            _brandViewModelBuilder = brandViewModelBuilder;
            _brandListViewModelBuilder = brandListViewModelBuilder;
        }

        [HttpGet]
        public async Task<ActionResult> Index(PageModel currentPageModel)
        {
            var model = await _brandViewModelBuilder.BuildAsync(currentPageModel);
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> List(PageModel currentPageModel)
        {
            var model = await _brandListViewModelBuilder.BuildAsync(currentPageModel);
            return View(model);
        }
    }
}
