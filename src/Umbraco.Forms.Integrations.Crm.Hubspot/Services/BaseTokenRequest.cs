using Newtonsoft.Json;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Services;

internal abstract class BaseTokenRequest
{
    public abstract string GrantType { get; }

    [JsonProperty("client_id")]
    public string ClientId { get; set; }

    [JsonProperty("redirect_uri")]
    public string RedirectUrl { get; set; }
}
