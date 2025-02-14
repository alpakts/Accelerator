using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.ViewModels.Persons;
using Litium.Runtime.AutoMapper;
using Litium.Sales;

namespace Litium.Accelerator.ViewModels.Checkout
{
    public class CustomerDetailsViewModel : AddressViewModel, IAutoMapperConfiguration
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Address, CustomerDetailsViewModel>()
                .ForMember(x => x.Address, m => m.MapFrom(address => address.Address1))
                .ForMember(x => x.PhoneNumber, m => m.MapFrom(address => address.MobilePhone))
               .ReverseMap()
               .IncludeBase<AddressViewModel, Address>();
        }
    }
}
