using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

public class AccountCollectionResponseDto
{
    [JsonPropertyName("accounts")]
    public List<AccountDto> Accounts { get; set; }
}
