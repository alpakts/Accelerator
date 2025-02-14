using Microsoft.AspNetCore.Mvc;
using Litium.Accelerator.Builders.Search;
using System.Threading.Tasks;

namespace Litium.Accelerator.Mvc.Controllers.Search
{
	public class SearchController : ControllerBase
	{
        private readonly SearchResultViewModelBuilder _searchResultViewModelBuilder;

        public SearchController(SearchResultViewModelBuilder searchResultViewModelBuilder)
        {
            _searchResultViewModelBuilder = searchResultViewModelBuilder;
        }

        public async Task<ActionResult> Index()
		{
			return View(await _searchResultViewModelBuilder.BuildAsync());
		}
	}
}
