using System.Threading.Tasks;
using Litium.Accelerator.Builders.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Api
{
    [Route("api/navigation")]
    public class NavigationController : ApiControllerBase
    {
        private readonly NavigationViewModelBuilder _navigationViewModelBuilder;

        public NavigationController(NavigationViewModelBuilder navigationViewModelBuilder)
        {
            _navigationViewModelBuilder = navigationViewModelBuilder;
        }

        /// <summary>
        /// Get navigation menu.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _navigationViewModelBuilder.BuildAsync());
        }
    }
}
