
using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

public class ContactMappingDto
{
    [JsonPropertyName("contactField")]
    public string ContactField { get; set; }

    [JsonPropertyName("formField")]
    public FormFieldDto FormField { get; set; }
}
