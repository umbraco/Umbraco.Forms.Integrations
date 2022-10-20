using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos
{
    public class CustomFieldCollectionResponseDto
    {
        [JsonPropertyName("fields")]
        public List<CustomFieldDto> Fields { get; set; }
    }
}
