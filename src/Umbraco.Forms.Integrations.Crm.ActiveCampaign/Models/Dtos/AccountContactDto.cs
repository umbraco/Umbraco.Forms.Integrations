using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos
{
    public class AccountContactDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("contact")]
        public int ContactId { get; set; }

        [JsonPropertyName("account")]
        public int AccountId { get; set; }
    }
}
