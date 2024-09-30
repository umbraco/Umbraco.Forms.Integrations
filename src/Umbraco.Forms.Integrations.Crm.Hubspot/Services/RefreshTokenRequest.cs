using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Services
{
    internal class RefreshTokenRequest : BaseTokenRequest
    {
        [JsonPropertyName("grant_type")]
        public override string GrantType => "refresh_token";

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
