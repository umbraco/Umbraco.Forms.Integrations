using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos
{
    public class ContactDetailDto
    {
        [JsonPropertyName("contact")]
        public ContactDto Contact { get; set; }
    }
}
