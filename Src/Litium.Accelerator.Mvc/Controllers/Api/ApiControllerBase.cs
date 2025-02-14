using Litium.Web.WebApi;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Api
{
    /// <summary>
    /// Api controller base class.
    /// </summary>
    [ApiCollection("site")]
    [ApiController]
    public abstract class ApiControllerBase : Controller
    {
    }
}
