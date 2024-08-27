using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Models.Dtos
{
    public class FormDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
