using System.Collections.Generic;
using Litium.Customers;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Definitions
{
    [Service(ServiceType = typeof(RoleSetup))]
    public abstract class RoleSetup
    {
        public abstract IEnumerable<Role> GetRoles();
    }
}
