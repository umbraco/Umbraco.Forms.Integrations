using Newtonsoft.Json;
using System.Collections.Generic;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Models.Responses
{
    public class PropertiesResponse
    {
        [JsonProperty(PropertyName = "results")]
        public List<Property> Results { get; set; }
    }
}
