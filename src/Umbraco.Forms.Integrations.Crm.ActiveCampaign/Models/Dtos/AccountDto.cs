using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos
{
    public class AccountDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
