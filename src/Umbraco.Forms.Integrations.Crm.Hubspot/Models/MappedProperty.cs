using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Models
{
    public class MappedProperty
    {
        [JsonPropertyName("formField")]
        public string FormField { get; set; }

        [JsonPropertyName("hubspotField")]
        public string HubspotField { get; set; }

        [JsonPropertyName("appendValue")]
        public bool AppendValue { get; set; }
    }
}
