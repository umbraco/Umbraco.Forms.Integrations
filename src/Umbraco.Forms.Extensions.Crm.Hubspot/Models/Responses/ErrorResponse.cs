using Newtonsoft.Json;

namespace Umbraco.Forms.Extensions.Crm.Hubspot.Models.Responses
{
    public class ErrorResponse
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}
