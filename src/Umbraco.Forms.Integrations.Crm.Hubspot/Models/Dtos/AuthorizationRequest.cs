using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Models.Dtos
{
    public class AuthorizationRequest
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
    }
}
