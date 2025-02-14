using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Builders
{
    [Service(Lifetime = DependencyLifetime.Scoped)]
    public interface IViewModelBuilder<T> where T : IViewModel
    {
    }
}
