using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Models.Dtos
{
    public class SubscriptionDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("entityId")]
        public string EntityId { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("typeName")] 
        public string TypeName => nameof(Constants.EntityType.Form);
        
        [JsonPropertyName("hookUrl")]
        public string HookUrl { get; set; }

        [JsonPropertyName("subscribeHook")]
        public bool SubscribeHook { get; set; }
    }
}
