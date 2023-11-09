using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

public class AccountContactRequestDto
{
    [JsonPropertyName("accountContact")]
    public AccountContactDto AccountContact { get; set; }
}
