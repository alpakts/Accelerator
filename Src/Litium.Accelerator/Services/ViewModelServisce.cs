using Litium.Accelerator.Builders;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Services
{
    [Service(Lifetime = DependencyLifetime.Transient)]
    public abstract class ViewModelService<T> where T : IViewModel
    {
    }
}

