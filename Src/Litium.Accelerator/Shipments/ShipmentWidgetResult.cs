using System;
using Newtonsoft.Json;

namespace Litium.Accelerator.Shipments
{
    public class ShipmentWidgetResult
    {
        public string ResponseString { get; set; }
        public string Id { get; set; }

        [JsonProperty("_force_update")]
        private Guid Ignored => Guid.NewGuid();
    }
}
