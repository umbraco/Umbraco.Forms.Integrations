using Newtonsoft.Json;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Models.Dtos
{
    public class SubscriptionDto
    {
        [JsonProperty("hookUrl")]
        public string HookUrl { get; set; }

        [JsonProperty("enable")]
        public bool Enable { get; set; }
    }
}
