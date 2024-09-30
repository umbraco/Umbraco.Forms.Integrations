using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Services
{
    internal abstract class BaseTokenRequest
    {
        public abstract string GrantType { get; }

        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }

        [JsonPropertyName("redirect_uri")]
        public string RedirectUrl { get; set; }
    }
}
