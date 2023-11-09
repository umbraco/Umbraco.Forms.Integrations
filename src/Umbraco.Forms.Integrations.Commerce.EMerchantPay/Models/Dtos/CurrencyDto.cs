using Newtonsoft.Json;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos;

public class CurrencyDto
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
}
