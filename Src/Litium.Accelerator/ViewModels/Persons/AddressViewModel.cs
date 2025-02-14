using AutoMapper;
using JetBrains.Annotations;
using Litium.Customers;
using Litium.Runtime.AutoMapper;
using System;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Persons
{
    public class AddressViewModel : IAutoMapperConfiguration, IViewModel
    {
        public Guid SystemId { get; set; } = Guid.NewGuid();
        public string AddressType { get; set; }
        public string CareOf { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Address, AddressViewModel>()
                .ForMember(x => x.Address, m => m.MapFrom(address => address.Address1))
                .ForMember(x => x.AddressType, m => m.MapFrom<AddressTypePropertyResolver>())
                .ReverseMap()
                .ForMember(x => x.SystemId, m => m.Ignore())
                .ForMember(x => x.Address1, m => m.MapFrom(address => address.Address))
                .ForMember(x => x.AddressTypeSystemId, m => m.MapFrom<AddressTypeSystemIdPropertyResolver>());

            cfg.CreateMap<AddressViewModel, Litium.Sales.Address>()
                .ForMember(x => x.Address1, m => m.MapFrom(address => address.Address))
                .ForMember(x => x.MobilePhone, m => m.MapFrom(address => address.PhoneNumber));
        }

        [UsedImplicitly]
        private class AddressTypePropertyResolver : IValueResolver<Address, AddressViewModel, string>
        {
            private readonly AddressTypeService _addressTypeService;

            public AddressTypePropertyResolver(AddressTypeService addressTypeService)
            {
                _addressTypeService = addressTypeService;
            }

            public string Resolve(Address source, AddressViewModel destination, string destMember, ResolutionContext context)
            {
                var addressType = _addressTypeService.Get(source.AddressTypeSystemId);

                return addressType?.Id;
            }
        }

        [UsedImplicitly]
        private class AddressTypeSystemIdPropertyResolver : IValueResolver<AddressViewModel, Address, Guid>
        {
            private readonly AddressTypeService _addressTypeService;

            public AddressTypeSystemIdPropertyResolver(AddressTypeService addressTypeService)
            {
                _addressTypeService = addressTypeService;
            }

            public Guid Resolve(AddressViewModel source, Address destination, Guid destMember, ResolutionContext context)
            {
                if (string.IsNullOrWhiteSpace(source.AddressType))
                {
                    return destination.AddressTypeSystemId;
                }

                var addressType = _addressTypeService.Get(source.AddressType);
                return addressType.SystemId;
            }
        }
    }
}
