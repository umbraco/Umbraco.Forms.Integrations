using Newtonsoft.Json;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Models;

public class MappedProperty
{
    [JsonProperty(PropertyName = "formField")]
    public string FormField { get; set; }

    [JsonProperty(PropertyName = "hubspotField")]
    public string HubspotField { get; set; }

    [JsonProperty(PropertyName = "appendValue")]
    public bool AppendValue { get; set; }
}
