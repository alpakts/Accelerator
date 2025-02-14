#if DEBUG
using Litium.Web.WebApi;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Api
{
    [Route("api/test")]
    public class TestController : ApiControllerBase
    {
        [Route("ping")]
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Get()
        {
            return Ok("pong");
        }

        [Route("ping")]
        [HttpPost]
        [FunctionOperationAuthorization("Function/SystemSettings")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Post()
        {
            return Ok("pong");
        }
    }
}
#endif
