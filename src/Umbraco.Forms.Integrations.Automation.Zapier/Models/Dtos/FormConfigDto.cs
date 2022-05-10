using Newtonsoft.Json;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Models.Dtos
{
    public class FormConfigDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("formName")]
        public string FormName { get; set; }

        [JsonProperty("hookUrl")]
        public string HookUrl { get; set; }
    }
}
