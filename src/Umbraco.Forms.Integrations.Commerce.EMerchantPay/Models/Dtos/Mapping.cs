using Newtonsoft.Json;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos;

public class Mapping
{
    [JsonProperty("customerProperty")]
    public string CustomerProperty { get; set; }

    [JsonProperty("field")]
    public MappingField Field { get; set; }
}

public class MappingField
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("value")]
    public string Value { get; set; }
}
