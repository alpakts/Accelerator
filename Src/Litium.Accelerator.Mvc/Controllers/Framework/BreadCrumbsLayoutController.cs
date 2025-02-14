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
    public class BreadCrumbsLayoutController : ViewComponent
    {
        private readonly BreadCrumbsViewModelBuilder<BreadCrumbsViewModel> _breadCrumbsViewModelBuilder;

        public BreadCrumbsLayoutController(
            BreadCrumbsViewModelBuilder<BreadCrumbsViewModel> breadCrumbsViewModelBuilder)
        {
            _breadCrumbsViewModelBuilder = breadCrumbsViewModelBuilder;
        }

        /// <summary>
        /// Builds bread crumbs for the site
        /// </summary>
        /// <returns>Return view for the bread crumbs</returns>
        public IViewComponentResult Invoke()
        {
            var viewModel = _breadCrumbsViewModelBuilder.BuildBreadCrumbs(0);
            return View("~/Views/Framework/BreadCrumbs.cshtml", viewModel);
        }
    }
}
