using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbraco.Forms.Integrations.Crm.Hubspot;

internal class PropertiesRequestV3
{
    [JsonProperty(PropertyName = "properties")]
    public JObject Properties { get; set; } = new JObject();
}
