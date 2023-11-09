using Newtonsoft.Json;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Models.Responses;

public class Property
{
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }

    [JsonProperty(PropertyName = "label")]
    public string Label { get; set; }

    [JsonProperty(PropertyName = "description")]
    public string Description { get; set; }
}
