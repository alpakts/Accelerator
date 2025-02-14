using System.Threading.Tasks;
using System.Web;
using Litium.Accelerator.Builders.Search;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Api
{
    [Route("api/quicksearch")]
    public class QuickSearchController : ApiControllerBase
    {
        private readonly QuickSearchResultViewModelBuilder _quickSearchResultViewModelBuilder;

        public QuickSearchController(QuickSearchResultViewModelBuilder quickSearchResultViewModelBuilder)
        {
            _quickSearchResultViewModelBuilder = quickSearchResultViewModelBuilder;
        }

        /// <summary>
        /// Search all data including product, category, page.
        /// </summary>
        /// <param name="query">The query.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("")]
        public async Task<IActionResult> Post([FromBody]string query)
        {
            var result = await _quickSearchResultViewModelBuilder.BuildAsync(HttpUtility.UrlDecode(query));
            return Ok(result.Results);
        }
    }
}
