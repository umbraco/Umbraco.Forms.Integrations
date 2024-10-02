using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Services
{
    internal class TokenResponse
    {
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresInSeconds { get; set; }
    }
}
