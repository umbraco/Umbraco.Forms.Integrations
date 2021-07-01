using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbraco.Forms.Extensions.Crm.Hubspot
{
    internal class PropertiesRequest
    {
        [JsonProperty(PropertyName = "properties")]
        public JObject Properties { get; set; } = new JObject();
    }
}
