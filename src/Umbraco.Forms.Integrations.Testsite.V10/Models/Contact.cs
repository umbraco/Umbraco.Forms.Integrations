using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Testsite.V10.Models;

public class Contact
{
    public Contact(string firstName, string lastName, string email, string company)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Company = company;
    }

    [JsonPropertyName("firstname")]
    public string FirstName { get; set; }

    [JsonPropertyName("lastname")]
    public string LastName { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("company")]
    public string Company { get; set; }
}
