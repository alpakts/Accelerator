using System.Collections.Generic;
using Litium.Accelerator.Builders.Product;
using Litium.Products;
using Litium.Web.Models.Products;
using Microsoft.AspNetCore.Mvc;
using Litium.Web.Rendering;
using System.Threading.Tasks;

namespace Litium.Accelerator.Mvc.Controllers.Product
{
    public class ProductController : ControllerBase
    {
        private readonly ProductPageViewModelBuilder _productPageViewModelBuilder;
        private readonly IEnumerable<IRenderingValidator<BaseProduct>> _renderingValidators;

        public ProductController(
            ProductPageViewModelBuilder productPageViewModelBuilder,
            IEnumerable<IRenderingValidator<BaseProduct>> renderingValidators)
        {
            _productPageViewModelBuilder = productPageViewModelBuilder;
            _renderingValidators = renderingValidators;
        }

        [HttpGet]
        public async Task<ActionResult> ProductWithVariants(ProductModel productModel)
        {
            var variant = productModel.SelectedVariant;
            if (variant is null)
            {
                return NotFound();
            }

            var baseProduct = productModel.BaseProduct;
            if (baseProduct is null || !_renderingValidators.Validate(baseProduct))
            {
                return NotFound();
            }
            var productPageModel = await _productPageViewModelBuilder.BuildAsync(variant);
            return View(productPageModel);
        }

        [HttpGet]
        public async Task<ActionResult> ProductWithVariantListing(ProductModel productModel)
        {
            var baseProduct = productModel.BaseProduct;
            if (baseProduct is null || !_renderingValidators.Validate(baseProduct))
            {
                return NotFound();
            }

            var productPageModel = await _productPageViewModelBuilder.BuildAsync(productModel.BaseProduct);
            return View(productPageModel);
        }
    }
}
