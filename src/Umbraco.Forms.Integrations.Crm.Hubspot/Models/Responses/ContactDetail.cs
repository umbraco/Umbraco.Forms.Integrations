using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Models.Responses;

public class ContactDetail
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("properties")]
    public Contact Properties { get; set; }
}
