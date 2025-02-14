using System;
using System.Collections.Generic;
using Litium.Accelerator.Services;
using Litium.Accelerator.ViewModels.Persons;
using Litium.Runtime.AutoMapper;

namespace Litium.Accelerator.Builders.MyPages
{
    public class BusinessAddressViewModelBuilder : IViewModelBuilder<AddressViewModel>
    {
        private readonly AddressViewModelService _addressViewModelService;

        public BusinessAddressViewModelBuilder(AddressViewModelService addressViewModelService)
        {
            _addressViewModelService = addressViewModelService;
        }

        public virtual IEnumerable<AddressViewModel> Build()
        {
            return _addressViewModelService.GetAddresses().MapEnumerableTo<AddressViewModel>();
        }

        public virtual AddressViewModel Build(Guid systemId)
        {
            return _addressViewModelService.GetAddress(systemId).MapTo<AddressViewModel>();
        }
    }
}
