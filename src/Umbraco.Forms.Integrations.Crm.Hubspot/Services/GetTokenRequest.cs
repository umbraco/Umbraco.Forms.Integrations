using Newtonsoft.Json;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Services;

internal class GetTokenRequest : BaseTokenRequest
{
    [JsonProperty("grant_type")]
    public override string GrantType => "authorization_code";
    
    [JsonProperty("code")]
    public string AuthorizationCode { get; set; }
}
