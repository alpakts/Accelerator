using System.ComponentModel;
using System.Threading.Tasks;
using Litium.Accelerator.Builders.Search;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Navigation
{
    [Browsable(false)]
    public class SubNavigationController : ViewComponent
    {
        private readonly SubNavigationViewModelBuilder _subNavigationViewModelBuilder;

        public SubNavigationController(SubNavigationViewModelBuilder subNavigationViewModelBuilder)
        {
            _subNavigationViewModelBuilder = subNavigationViewModelBuilder;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View("~/Views/Navigation/SubNavigationCategory.cshtml", await _subNavigationViewModelBuilder.BuildAsync());
        }
    }
}
