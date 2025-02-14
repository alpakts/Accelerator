using System.Threading;
using System.Threading.Tasks;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Definitions
{
    [Service(ServiceType = typeof(IDefinitionSetup), Lifetime = DependencyLifetime.Transient)]
    public interface IDefinitionSetup
    {
        ValueTask StartAsync(CancellationToken cancellationToken);
    }
}
