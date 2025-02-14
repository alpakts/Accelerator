using Newtonsoft.Json;

namespace Litium.Accelerator.ViewModels.Checkout
{
    public class KlarnaOnChangeViewModel
    {
        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("family_name")]
        public string FamilyName { get; set; }

        [JsonProperty("given_name")]
        public string GivenName { get; set; }

        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }
    }
}
