using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Constants;
using Litium.Accelerator.ViewModels.Persons;
using Litium.Customers;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Websites;
using System.Collections.Generic;
using System.Linq;
using Litium.Accelerator.Builders;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Litium.Accelerator.ViewModels.MyPages
{
    public class MyDetailsViewModel : IAutoMapperConfiguration, IViewModel
    {
        public string CustomerNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public AddressViewModel DeliveryAddress { get; set; }
        public AddressViewModel AlternativeDeliveryAddress { get; set; }

        public IEnumerable<SelectListItem> Countries { get; set; }
        public bool IsSystemAccount{ get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, MyDetailsViewModel>();
            cfg.CreateMap<Person, MyDetailsViewModel>()
               .ForMember(x => x.CustomerNumber, m => m.MapFrom(person => person.Id))
               .ForMember(x => x.DeliveryAddress, m => m.MapFrom<AddressViewModelResolver>())
               .ForMember(x => x.AlternativeDeliveryAddress, m => m.MapFrom<AlternativeAddressViewModelResolver>())
               .ReverseMap()
               .ForMember(x => x.Addresses, m => m.MapFrom<AddressModelResolver>());
        }

        private class AddressViewModelResolver : IValueResolver<Person, MyDetailsViewModel, AddressViewModel>
        {
            private readonly AddressTypeService _addressTypeService;

            public AddressViewModelResolver(AddressTypeService addressTypeService)
            {
                _addressTypeService = addressTypeService;
            }

            public AddressViewModel Resolve(Person source, MyDetailsViewModel destination, AddressViewModel destMember, ResolutionContext context)
            {
                var addressType = _addressTypeService.Get(AddressTypeNameConstants.Address);
                var address = source.Addresses.FirstOrDefault(x => x.AddressTypeSystemId == addressType.SystemId);
                if (address != null)
                {
                    return address.MapTo<AddressViewModel>();
                }

                return new AddressViewModel();
            }
        }

        private class AlternativeAddressViewModelResolver : IValueResolver<Person, MyDetailsViewModel, AddressViewModel>
        {
            private readonly AddressTypeService _addressTypeService;

            public AlternativeAddressViewModelResolver(AddressTypeService addressTypeService)
            {
                _addressTypeService = addressTypeService;
            }

            public AddressViewModel Resolve(Person source, MyDetailsViewModel destination, AddressViewModel destMember, ResolutionContext context)
            {
                var alternativeAddressType = _addressTypeService.Get(AddressTypeNameConstants.AlternativeAddress);
                var alternativeAddress = source.Addresses.FirstOrDefault(x => x.AddressTypeSystemId == alternativeAddressType.SystemId);
                if (alternativeAddress != null)
                {
                    return alternativeAddress.MapTo<AddressViewModel>();
                }

                return new AddressViewModel();
            }
        }

        private class AddressModelResolver : IValueResolver<MyDetailsViewModel, Person, IList<Address>>
        {
            private readonly AddressTypeService _addressTypeService;

            public AddressModelResolver(AddressTypeService addressTypeService)
            {
                _addressTypeService = addressTypeService;
            }

            public IList<Address> Resolve(MyDetailsViewModel source, Person destination, IList<Address> destMember, ResolutionContext context)
            {
                List<Address> addresses = new List<Address>(destination.Addresses);

                var addressType = _addressTypeService.Get(AddressTypeNameConstants.Address);
                var address = addresses.FirstOrDefault(x => x.AddressTypeSystemId == addressType.SystemId);
                if (address == null)
                {
                    address = new Address(addressType.SystemId);
                    addresses.Add(address);
                }
                address.MapFrom(source.DeliveryAddress);

                var alternativeAddressType = _addressTypeService.Get(AddressTypeNameConstants.AlternativeAddress);
                var alternativeAddress = addresses.FirstOrDefault(x => x.AddressTypeSystemId == alternativeAddressType.SystemId);
                if (alternativeAddress == null)
                {
                    alternativeAddress = new Address(alternativeAddressType.SystemId);
                    addresses.Add(alternativeAddress);
                }
                alternativeAddress.MapFrom(source.AlternativeDeliveryAddress);

                return addresses;
            }
        }
    }
}
