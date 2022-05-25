
using Newtonsoft.Json;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Models.Dtos
{
    public class FormDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
