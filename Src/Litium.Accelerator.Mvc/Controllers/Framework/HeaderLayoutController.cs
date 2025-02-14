using Litium.Accelerator.ViewModels.Framework;
using Litium.Accelerator.Builders.Framework;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Litium.Accelerator.Mvc.Controllers.Framework
{
    /// <summary>
    /// LayoutController renders views for the layout (header,foter,BreadCrumbs)
    /// </summary>
    [Browsable(false)]
    public class HeaderLayoutController : ViewComponent
    {
        private readonly HeaderViewModelBuilder<HeaderViewModel> _headerViewModelBuilder;

        public HeaderLayoutController(HeaderViewModelBuilder<HeaderViewModel> headerViewModelBuilder)
        {
            _headerViewModelBuilder = headerViewModelBuilder;
        }

        /// <summary>
        /// Builds header for the site
        /// </summary>
        /// <returns>Return view for the header</returns>
        public IViewComponentResult Invoke()
        {
            var viewModel = _headerViewModelBuilder.Build();
            return View("~/Views/Framework/Header.cshtml", viewModel);
        }
   }
}
