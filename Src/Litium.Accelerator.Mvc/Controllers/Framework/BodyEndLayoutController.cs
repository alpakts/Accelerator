using System.ComponentModel;
using Litium.Accelerator.Builders.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Framework
{
    /// <summary>
    /// LayoutController renders views for the layout (header,foter,BreadCrumbs)
    /// </summary>
    [Browsable(false)]
    public class BodyEndLayoutController : ViewComponent
    {
        private readonly BodyViewModelBuilder _bodyViewModelBuilder;

        public BodyEndLayoutController(
            BodyViewModelBuilder bodyViewModelBuilder)
        {
            _bodyViewModelBuilder = bodyViewModelBuilder;
        }

        public IViewComponentResult Invoke()
        {
            var viewModel = _bodyViewModelBuilder.BuildBodyEnd();
            return View("~/Views/Framework/BodyEnd.cshtml", viewModel);
        }
   }
}
