using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Models.Responses;

public class Contact
{
    [JsonPropertyName("firstname")]
    public string FirstName { get; set; }

    [JsonPropertyName("lastname")]
    public string LastName { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("company")]
    public string Company { get; set; }
}
