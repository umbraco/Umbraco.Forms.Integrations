using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

public class CustomFieldValueDto
{
    [JsonPropertyName("field")]
    public string Field { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}
