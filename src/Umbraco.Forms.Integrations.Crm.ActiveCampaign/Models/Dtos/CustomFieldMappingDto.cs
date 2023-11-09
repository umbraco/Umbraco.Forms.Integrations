
using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

public class CustomFieldMappingDto
{
    [JsonPropertyName("customField")]
    public CustomFieldDto CustomField { get; set; }

    [JsonPropertyName("formField")]
    public FormFieldDto FormField { get; set; }
}
