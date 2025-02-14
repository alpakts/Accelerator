using System.Collections.Generic;
using Litium.Customers;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Definitions
{
    [Service(ServiceType = typeof(AddressTypeSetup))]
    public abstract class AddressTypeSetup
    {
        public abstract IEnumerable<AddressType> GetAddressTypes();
    }
}
