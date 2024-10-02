using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot
{
    internal class PropertiesRequestV3
    {
        [JsonPropertyName("properties")]
        public JsonObject Properties { get; set; } = new JsonObject();
    }
}
