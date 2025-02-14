using AutoMapper;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Globalization;
using Litium.Accelerator.Builders;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models;
using Litium.Sales;
using Litium.Web.Models.Products;
using Litium.Globalization;

namespace Litium.Accelerator.ViewModels.Order
{
    public class OrderDetailsViewModel : IAutoMapperConfiguration, IViewModel
    {
        public Guid OrderId { get; set; }
        public string ExternalOrderID { get; set; }
        public string OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public string Status { get; set; }
        public string OrderTotalFee { get; set; }
        public string OrderTotalDiscountAmount { get; set; }
        public string OrderGrandTotal { get; set; }
        public string OrderTotalVat { get; set; }
        public string OrderTotalDeliveryCost { get; set; }
        [NotNull]
        public IList<OrderRowItem> OrderRows { get; set; } = new List<OrderRowItem>();
        public string PaymentMethod { get; set; }
        public string DeliveryMethod { get; set; }
        public DateTimeOffset? ActualDeliveryDate { get; set; }
        public string FormattedActualDeliveryDate { get => ActualDeliveryDate.HasValue ? ActualDeliveryDate.Value.DateTime.ToShortDateString() : string.Empty;}
        [NotNull]
        public IList<DeliveryItem> Deliveries { get; set; } = new List<DeliveryItem>();

        public IList<OrderRowItem> DiscountRows { get; set; } = new List<OrderRowItem>();

        public CustomerInfoModel CustomerInfo { get; set; } = new CustomerInfoModel();
        public string CompanyName { get; set; }
        public bool IsWaitingApprove { get; set; }

        public string MerchantOrganizationNumber { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Address, DeliveryItem.AddressItem>()
               .ForMember(x => x.Zip, m => m.MapFrom(x => x.ZipCode))
               .ForMember(x => x.Country, m => m.MapFrom<CountryNameResolver>());
        }

        public class CustomerInfoModel
        {
            public string CustomerNumber { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Address1 { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
            public string Zip { get; set; }
        }

        public class OrderRowItem
        {
            public string Brand { get; set; }
            public string Name { get; set; }
            public string QuantityString { get; set; }
            public ProductPriceModel PriceInfo { get; set; }
            public string TotalPrice { get; set; }
            public Guid DeliveryId { get; set; }
            public LinkModel Link { get; set; }
            public bool IsFreeGift { get; set; }
        }

        public class DeliveryItem
        {
            public Guid DeliveryId { get; set; }
            public AddressItem Address { get; set; } = new AddressItem();
            public string DeliveryMethodTitle { get; set; }
            public string DeliveryRowTotalCost { get; set; }

            public class AddressItem
            {
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public string Address1 { get; set; }
                public string Address2 { get; set; }
                public string City { get; set; }
                public string Country { get; set; }
                public string Zip { get; set; }
                public string MobilePhone { get; set; }
            }
        }

        public class OrderDetailsLabel
        {
            public string AcceptConditionsText { get; set; }
            public string TermsAndAgreement { get; set; }
        }

        private sealed class CountryNameResolver : IValueResolver<Address, DeliveryItem.AddressItem, string>
        {
            private readonly IsoCountryService _isoCountryService;

            public CountryNameResolver(IsoCountryService isoCountryService)
            {
                _isoCountryService = isoCountryService;
            }

            public string Resolve(Address source, DeliveryItem.AddressItem destination, string destMember, ResolutionContext context)
            {
                if (string.IsNullOrWhiteSpace(source.Country))
                {
                    return string.Empty;
                }

                return _isoCountryService.Get(source.Country)?.EnglishName;
            }
        }
    }
}

