using System.Threading;
using System.Threading.Tasks;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Definitions
{
    [Service(ServiceType = typeof(IDefinitionInit), Lifetime = DependencyLifetime.Transient)]
    public interface IDefinitionInit
    {
        ValueTask StartAsync(CancellationToken cancellationToken);
    }
}
