using System.ComponentModel;
using System.Threading.Tasks;
using Litium.Accelerator.Builders.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Framework
{
    [Browsable(false)]
    public class ClientContextController : ViewComponent
    {
        private readonly ClientContextViewModelBuilder _clientContextViewModelBuilder;

        public ClientContextController(ClientContextViewModelBuilder clientContextViewModelBuilder)
        {
            _clientContextViewModelBuilder = clientContextViewModelBuilder;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var viewModel = await _clientContextViewModelBuilder.BuildAsync();
            return View("~/Views/Framework/ClientContext.cshtml", viewModel);
        }
    }
}
