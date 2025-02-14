using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Litium.Accelerator.Builders.Product;
using Litium.Web.Models.Products;
using Litium.Web.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Category
{
    public class CategoryController : ControllerBase
    {
        private readonly CategoryPageViewModelBuilder _categoryPageViewModelBuilder;
        private readonly IEnumerable<IRenderingValidator<Products.Category>> _renderingValidators;

        public CategoryController(
            CategoryPageViewModelBuilder categoryPageViewModelBuilder,
            IEnumerable<IRenderingValidator<Products.Category>> renderingValidators)
        {
            _categoryPageViewModelBuilder = categoryPageViewModelBuilder;
            _renderingValidators = renderingValidators;
        }

        public async Task<ActionResult> Index(CategoryModel currentCategoryModel)
        {
            if (!_renderingValidators.Validate(currentCategoryModel.Category))
            {
                return NotFound();
            }

            var model = await _categoryPageViewModelBuilder.BuildAsync(currentCategoryModel.SystemId);

            return View(model);
        }
    }
}
