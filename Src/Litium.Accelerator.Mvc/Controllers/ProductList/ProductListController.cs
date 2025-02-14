using System.Threading.Tasks;
using Litium.Accelerator.Builders.Product;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.ProductList
{
    public class ProductListController : ControllerBase
    {
        private readonly ProductListViewModelBuilder _productListViewModelBuilder;

        public ProductListController(ProductListViewModelBuilder productListViewModelBuilder)
        {
            _productListViewModelBuilder = productListViewModelBuilder;
        }

        public async Task<ActionResult> Index()
        {
            return View(await _productListViewModelBuilder.BuildAsync());
        }
    }
}
