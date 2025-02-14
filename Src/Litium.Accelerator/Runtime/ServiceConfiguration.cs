using Litium.Accelerator.Configuration;
using Litium.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Litium.Accelerator.Runtime
{
    internal class ServiceConfiguration :
        IApplicationConfiguration
    {
        public void Configure(ApplicationConfigurationBuilder app)
        {
            app.ConfigureServices(service =>
            {
                service.Configure<SmtpConfig>(app.Configuration.GetSection("Litium:Accelerator:Smtp"));
            });
        }
    }
}
