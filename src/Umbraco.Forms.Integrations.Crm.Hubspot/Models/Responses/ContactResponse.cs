using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Models.Responses;

public class ContactResponse
{
    [JsonPropertyName("results")]
    public List<ContactDetail> Results { get; set; }
}
