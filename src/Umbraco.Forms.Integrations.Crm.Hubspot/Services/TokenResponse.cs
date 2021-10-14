using Newtonsoft.Json;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Services
{
    internal class TokenResponse
    {
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresInSeconds { get; set; }
    }
}
