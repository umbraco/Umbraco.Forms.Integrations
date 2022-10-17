using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos
{
    public class ContactResponseDto
    {
        [JsonPropertyName("contact")]
        public ContactDto Contact { get; set; }
    }
}
