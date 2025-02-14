using AutoMapper;
using Litium.Runtime.AutoMapper;

namespace Litium.Accelerator.Mvc.Runtime
{
    internal class AddressMapper : IAutoMapperConfiguration
    {
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Sales.Address, Customers.Address>();
        }
    }
}
