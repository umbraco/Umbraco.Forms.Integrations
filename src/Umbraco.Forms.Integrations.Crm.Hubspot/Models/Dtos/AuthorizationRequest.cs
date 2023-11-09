using Newtonsoft.Json;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Models.Dtos;

public class AuthorizationRequest
{
    [JsonProperty("code")]
    public string Code { get; set; }
}
