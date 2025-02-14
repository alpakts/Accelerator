using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Utilities
{
    [Service(ServiceType = typeof(SessionStorage), Lifetime = DependencyLifetime.Singleton)]
    public abstract class SessionStorage
    {
        public abstract T GetValue<T>(string name);
        public abstract void SetValue<T>(string name, T value);
    }
}
