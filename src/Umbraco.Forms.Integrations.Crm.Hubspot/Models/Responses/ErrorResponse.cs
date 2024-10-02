using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Models.Responses
{
    public class ErrorResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
