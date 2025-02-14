using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Builders.Framework;
using Litium.Accelerator.ViewModels.Framework;
using Litium.Sales;
using Litium.Web;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Api
{
    [Route("api/cart")]
    public class CartController : ApiControllerBase
    {
        private readonly CartViewModelBuilder _cartViewModelBuilder;
        private readonly OrderService _orderService;

        public CartController(
            CartViewModelBuilder cartViewModelBuilder,
            OrderService orderService)
        {
            _cartViewModelBuilder = cartViewModelBuilder;
            _orderService = orderService;
        }

        /// <summary>
        /// Gets the current shopping cart.
        /// </summary>
        [Route("")]
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Get()
        {
            var cartContext = await HttpContext.GetCartContextAsync();
            return Ok(_cartViewModelBuilder.Build(cartContext));
        }

        /// <summary>
        /// Adds an article to the current shopping cart.
        /// </summary>
        /// <param name="model">
        ///     Object containing the article number that is used when the product is
        ///     published on the current web site and quantity of the article.
        /// </param>
        [HttpPost]
        [Route("add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddToCartViewModel model)
        {
            var cartContext = await HttpContext.GetCartContextAsync();
            await cartContext.AddOrUpdateItemAsync(new AddOrUpdateCartItemArgs
            {
                ArticleNumber = model.ArticleNumber,
                Quantity = model.Quantity,
            });

            return Ok(_cartViewModelBuilder.Build(cartContext));
        }

        /// <summary>
        /// Adds all articles from an order to the current shopping cart.
        /// </summary>
        /// <param name="model">Object containing the order system identifier that is used to get the articles.</param>
        [HttpPost]
        [Route("reorder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder(ReorderRequestViewModel model)
        {
            var order = _orderService.Get<SalesOrder>(model.OrderId);
            if (order == null || order.Rows.Count == 0)
            {
                ModelState.AddModelError("general", "cart.reorder.orderinvalid".AsWebsiteText());
                return BadRequest(ModelState);
            }

            var cartContext = await HttpContext.GetCartContextAsync();
            foreach (var orderRow in order.Rows.Where(x => x.OrderRowType == OrderRowType.Product))
            {
                await cartContext.AddOrUpdateItemAsync(new AddOrUpdateCartItemArgs()
                {
                    ArticleNumber = orderRow.ArticleNumber,
                    Quantity = orderRow.Quantity,
                });
            }

            return Ok(_cartViewModelBuilder.Build(cartContext));
        }

        /// <summary>
        /// Updates the quantity of an article and refresh the current shopping cart.
        /// </summary>
        /// <param name="model">Object containing the article number and the quantity.</param>
        [HttpPut]
        [Route("update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(AddToCartViewModel model)
        {
            var cartContext = await HttpContext.GetCartContextAsync();
            await cartContext.AddOrUpdateItemAsync(new AddOrUpdateCartItemArgs
            {
                ArticleNumber = model.ArticleNumber,
                Quantity = model.Quantity,
                ConstantQuantity = true,
            });
            await cartContext.CalculateShipmentsAsync();
            await cartContext.CalculatePaymentsAsync();

            return Ok(_cartViewModelBuilder.Build(cartContext));
        }
    }
}
