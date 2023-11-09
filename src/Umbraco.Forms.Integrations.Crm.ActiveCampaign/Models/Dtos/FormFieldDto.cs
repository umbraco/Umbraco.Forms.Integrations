
using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

public class FormFieldDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}
