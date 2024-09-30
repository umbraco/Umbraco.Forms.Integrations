using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Models.Responses
{
    public class PropertiesResponse
    {
        [JsonPropertyName("results")]
        public List<Property> Results { get; set; }
    }
}