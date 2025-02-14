using Litium.Accelerator.Builders.Product;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System;

namespace Litium.Accelerator.Mvc.Controllers.Navigation
{
    [Browsable(false)]
    public class ChildCategoryNavigationController : ViewComponent
    {
        private readonly ChildCategoryNavigationBuilder _childCategoryNavigationBuilder;

        public ChildCategoryNavigationController(ChildCategoryNavigationBuilder childCategoryNavigationBuilder)
        {
            _childCategoryNavigationBuilder = childCategoryNavigationBuilder;
        }

        public IViewComponentResult Invoke(Guid categorySystemId)
        {
            var model = _childCategoryNavigationBuilder.Build(categorySystemId);
            if (model != null)
            {
                return View("~/Views/Navigation/ChildCategoryNavigation.cshtml", model);
            }

            return Content("");
        }
    }
}
