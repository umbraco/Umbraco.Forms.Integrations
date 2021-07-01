using Newtonsoft.Json;

namespace Umbraco.Forms.Extensions.Crm.Hubspot.Models
{
    public class MappedProperty
    {
        [JsonProperty(PropertyName = "formField")]
        public string FormField { get; set; }

        [JsonProperty(PropertyName = "hubspotField")]
        public string HubspotField { get; set; }
    }
}
