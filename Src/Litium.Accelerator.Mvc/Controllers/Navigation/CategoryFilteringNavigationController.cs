using System.ComponentModel;
using Litium.Accelerator.Builders.Product;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Navigation
{
    [Browsable(false)]
    public class CategoryFilteringNavigationController : ViewComponent
    {
        private readonly CategoryFilteringViewModelBuilder _categoryFilteringViewModelBuilder;

        public CategoryFilteringNavigationController(CategoryFilteringViewModelBuilder categoryFilteringViewModelBuilder)
        {
            _categoryFilteringViewModelBuilder = categoryFilteringViewModelBuilder;
        }

        public IViewComponentResult Invoke(int totalHits = 0)
        {
            return View("~/Views/Navigation/CategoryFiltering.cshtml", _categoryFilteringViewModelBuilder.Build(totalHits));
        }
    }
}
