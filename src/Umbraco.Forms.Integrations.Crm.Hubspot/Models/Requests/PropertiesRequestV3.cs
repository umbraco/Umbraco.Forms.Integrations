using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot
{
    internal class PropertiesRequestV3
    {
        [JsonPropertyName("properties")]
        public JObject Properties { get; set; } = new JObject();
    }
}
