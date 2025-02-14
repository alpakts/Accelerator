using System;
using System.Collections.Generic;
using Litium.Accelerator.Builders;
using Litium.Accelerator.Payments;
using Litium.Accelerator.Shipments;
using Litium.Accelerator.ViewModels.Persons;
using Litium.Sales;

namespace Litium.Accelerator.ViewModels.Checkout
{
    public class CheckoutViewModel : IViewModel
    {
        public PaymentWidgetResult PaymentWidget { get; set; }
        public ShipmentWidgetResult ShipmentWidget { get; set; }

        public IList<DeliveryMethodViewModel> DeliveryMethods { get; set; }
        public IList<PaymentOptionViewModel> PaymentMethods { get; set; }
        public string SelectedCountry { get; set; }
        public DeliveryMethodViewModel SelectedDeliveryMethod { get; set; }
        public PaymentOptionViewModel SelectedPaymentMethod { get; set; }
        public ProviderOptionIdentifier DefaultDeliveryMethod { get; set; }
        public ProviderOptionIdentifier DefaultPaymentMethod { get; set; }
        public CustomerDetailsViewModel CustomerDetails { get; set; }
        public CustomerDetailsViewModel AlternativeAddress { get; set; }
        public IList<AddressViewModel> CompanyAddresses { get; set; }
        public Guid? SelectedCompanyAddressId { get; set; }
        public bool Authenticated { get; set; }
        public bool AcceptTermsOfCondition { get; set; }
        public bool ShowAlternativeAddress { get; set; }
        public int CheckoutMode { get; set; }
        public bool SignUp { get; set; }
        public bool IsBusinessCustomer { get; set; }
        public string OrderNote { get; set; }
        public string CompanyName { get; set; }
        public string DiscountCode { get; set; }
        public ISet<string> UsedDiscountCodes { get; set; }

        public string OrderId { get; set; }
        public string Payload { get; set; }
        public bool Success { get; set; }
        public Dictionary<string,List<string>> ErrorMessages { get; set; } = new Dictionary<string, List<string>>();

        public string CheckoutUrl { get; set; }
        public string TermsUrl { get; set; }
        public string LoginUrl { get; set; }
        public string RedirectUrl { get; set; }
        public string GiftCard { get; set; }
        public ISet<string> UsedGiftCards { get; set; }
        public string ShippingInfo { get; set; }
    }
}
