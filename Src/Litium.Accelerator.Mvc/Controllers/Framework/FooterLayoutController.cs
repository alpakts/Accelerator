using System.ComponentModel;
using Litium.Accelerator.Builders.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Framework
{
    /// <summary>
    /// LayoutController renders views for the layout (header,foter,BreadCrumbs)
    /// </summary>
    [Browsable(false)]
    public class FooterLayoutController : ViewComponent
    {
        private readonly FooterViewModelBuilder _footerViewModelBuilder;

        public FooterLayoutController(FooterViewModelBuilder footerViewModelBuilder)
        {
            _footerViewModelBuilder = footerViewModelBuilder;
        }

        public IViewComponentResult Invoke()
        {
            var viewModel = _footerViewModelBuilder.Build();
            return View("~/Views/Framework/Footer.cshtml", viewModel);
        }
    }
}
