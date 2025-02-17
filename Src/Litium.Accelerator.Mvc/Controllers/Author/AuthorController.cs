using Litium.Web.Models.Websites;
using Litium.Accelerator.Builders.Author;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Author
{
    public class AuthorController : ControllerBase
    {
        private readonly AuthorViewModelBuilder _AuthorViewModelBuilder;

        public AuthorController(AuthorViewModelBuilder AuthorViewModelBuilder)
        {
            _AuthorViewModelBuilder = AuthorViewModelBuilder;
        }

        [HttpGet]
        public ActionResult Index(PageModel currentPageModel)
        {
            var model = _AuthorViewModelBuilder.Build(currentPageModel);
            return View(model);
        }
    }
}