using System.ComponentModel;
using Litium.Accelerator.Builders.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Framework
{
    /// <summary>
    /// LayoutController renders views for the layout (header,foter,BreadCrumbs)
    /// </summary>
    [Browsable(false)]
    public class BodyStartLayoutController : ViewComponent
    {
        private readonly BodyViewModelBuilder _bodyViewModelBuilder;

        public BodyStartLayoutController(
            BodyViewModelBuilder bodyViewModelBuilder)
        {
            _bodyViewModelBuilder = bodyViewModelBuilder;
        }

        public IViewComponentResult Invoke()
        {
            var viewModel = _bodyViewModelBuilder.BuildBodyStart();
            return View("~/Views/Framework/BodyStart.cshtml", viewModel);
        }
    }
}
