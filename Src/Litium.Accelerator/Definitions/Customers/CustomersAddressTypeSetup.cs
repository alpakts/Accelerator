using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.Customers;

namespace Litium.Accelerator.Definitions.Customers
{
    internal class CustomersAddressTypeSetup : AddressTypeSetup
    {
        public override IEnumerable<AddressType> GetAddressTypes()
        {
            var items = new List<AddressType>
            {
                new AddressType()
                {
                    Id = AddressTypeNameConstants.Address,
                    Localizations =
                    {
                        ["sv-SE"] = { Name = AddressTypeNameConstants.Address },
                        ["en-US"] = { Name = AddressTypeNameConstants.Address },
                    }
                },
                new AddressType()
                {
                    Id = AddressTypeNameConstants.AlternativeAddress,
                    Localizations =
                    {
                        ["sv-SE"] = { Name = AddressTypeNameConstants.AlternativeAddress },
                        ["en-US"] = { Name = AddressTypeNameConstants.AlternativeAddress },
                    }
                },
                new AddressType()
                {
                    Id = AddressTypeNameConstants.Billing,
                    Localizations =
                    {
                        ["sv-SE"] = { Name = AddressTypeNameConstants.Billing },
                        ["en-US"] = { Name = AddressTypeNameConstants.Billing },
                    }
                },
                new AddressType()
                {
                    Id = AddressTypeNameConstants.Delivery,
                    Localizations =
                    {
                        ["sv-SE"] = { Name = AddressTypeNameConstants.Delivery },
                        ["en-US"] = { Name = AddressTypeNameConstants.Delivery },
                    }
                },
                new AddressType()
                {
                    Id = AddressTypeNameConstants.BillingAndDelivery,
                    Localizations =
                    {
                        ["sv-SE"] = { Name = AddressTypeNameConstants.BillingAndDelivery },
                        ["en-US"] = { Name = AddressTypeNameConstants.BillingAndDelivery },
                    }
                },
            };
            return items;
        }
    }
}
