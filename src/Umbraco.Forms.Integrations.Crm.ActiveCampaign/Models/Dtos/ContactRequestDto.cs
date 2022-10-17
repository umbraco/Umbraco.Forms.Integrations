using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos
{
    public class ContactRequestDto
    {
        [JsonPropertyName("contact")]
        public ContactDto Contact { get; set; }
    }
}
