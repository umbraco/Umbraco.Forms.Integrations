using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Services
{
    internal class GetTokenRequest : BaseTokenRequest
    {
        [JsonPropertyName("grant_type")]
        public override string GrantType => "authorization_code";
        
        [JsonPropertyName("code")]
        public string AuthorizationCode { get; set; }
    }
}
