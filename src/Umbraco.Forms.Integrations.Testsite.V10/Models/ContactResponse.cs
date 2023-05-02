using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Testsite.V10.Models;

public class ContactResponse
{
    [JsonPropertyName("results")]
    public List<ContactDetail> Results { get; set; }

    public ContactResponse(List<ContactDetail> results)
    {
        Results = results;
    }
}
