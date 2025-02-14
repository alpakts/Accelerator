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
    public class HeadLayoutController : ViewComponent
    {
        private readonly HeadViewModelBuilder<HeadViewModel> _headViewModelBuilder;

        public HeadLayoutController(HeadViewModelBuilder<HeadViewModel> headViewModelBuilder)
        {
            _headViewModelBuilder = headViewModelBuilder;
        }

        public IViewComponentResult Invoke()
        {
            var viewModel = _headViewModelBuilder.Build();
            return View("~/Views/Framework/Head.cshtml", viewModel);
        }
    }
}
