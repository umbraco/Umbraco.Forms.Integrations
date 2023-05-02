using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Testsite.V10.Models;

public class ContactDetail
{
    public ContactDetail(string id, Contact properties)
    {
        Id = id;
        Properties = properties;
    }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("properties")]
    public Contact Properties { get; set; }
}
