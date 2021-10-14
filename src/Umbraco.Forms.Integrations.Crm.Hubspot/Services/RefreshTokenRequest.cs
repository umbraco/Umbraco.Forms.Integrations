using Newtonsoft.Json;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Services
{
    internal class RefreshTokenRequest : BaseTokenRequest
    {
        [JsonProperty("grant_type")]
        public override string GrantType => "refresh_token";

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
