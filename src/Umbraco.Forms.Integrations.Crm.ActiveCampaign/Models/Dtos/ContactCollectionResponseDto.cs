using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos
{
    public class ContactCollectionResponseDto
    {
        [JsonPropertyName("contacts")]
        public List<ContactDto> Contacts { get; set; }
    }
}
