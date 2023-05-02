using Microsoft.AspNetCore.Mvc;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Forms.Integrations.Testsite.V10.Models;

namespace Umbraco.Forms.Integrations.Testsite.V10.Controllers;

[Route("umbraco/api/hubspot/v1/contacts")]
public class ContactsController : UmbracoApiController
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ContactsController> _logger;

    protected readonly string PrivateAccessToken;

    public ContactsController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<ContactsController> logger)
	{
		_httpClientFactory = httpClientFactory;
        _logger = logger;

        PrivateAccessToken = configuration.GetSection(Constants.HubspotSettingsPath)["PrivateAccessToken"];
	}

    /// <summary>
    /// Gets the list of contacts from HubSpot.
    /// </summary>
    /// <returns>A list of contacts or Unauthorized, BadRequest status codes.</returns>
    [HttpGet]
    [ServiceFilter(typeof(HubspotPrivateAccessTokenFilterAttribute))]
    public async Task<IActionResult> Get()
    {
        var client = _httpClientFactory.CreateClient(Constants.HubspotClient);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PrivateAccessToken);

        var response = await client.GetAsync("objects/contacts");
        if(!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to fetch contacts from HubSpot API. {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            return BadRequest(response.ReasonPhrase);
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        return Ok(JsonSerializer.Deserialize<ContactResponse>(responseContent));
    }

    /// <summary>
    /// Get a contact by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>The contact details or Unauthorized, BadRequest status codes.</returns>
    [HttpGet]
    [ServiceFilter(typeof(HubspotPrivateAccessTokenFilterAttribute))]
    [Route("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var client = _httpClientFactory.CreateClient(Constants.HubspotClient);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PrivateAccessToken);

        var response = await client.GetAsync($"objects/contacts/{id}");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to fetch contact with id: {id} from HubSpot API. {StatusCode} {ReasonPhrase}", id, response.StatusCode, response.ReasonPhrase);
            return BadRequest(response.ReasonPhrase);
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        return Ok(JsonSerializer.Deserialize<ContactDetail>(responseContent));
    }

    /// <summary>
    /// Create new contact.
    /// </summary>
    /// <param name="contact"></param>
    /// <returns>Created, Unauthorized or BadRequest status codes.</returns>
    [HttpPost]
    [ServiceFilter(typeof(HubspotPrivateAccessTokenFilterAttribute))]
    public async Task<IActionResult> Create([FromBody] ContactDetail contact)
    {
        var client = _httpClientFactory.CreateClient(Constants.HubspotClient);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PrivateAccessToken);

        var response = await client.PostAsync("objects/contacts", 
            new StringContent(JsonSerializer.Serialize(contact), Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to create contact. {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            return BadRequest(response.ReasonPhrase);
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var contactDetail = JsonSerializer.Deserialize<ContactDetail>(responseContent);
        if (contactDetail != null)
        {
            contact.Id = contactDetail.Id;
        }

        return Created($"objects/contacts/{(contact != null ? contact.Id : string.Empty)}", contact);
    }

    /// <summary>
    /// Update contact details.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="contact"></param>
    /// <returns>Success, Unauthorized or BadRequest status codes.</returns>
    [HttpPatch]
    [ServiceFilter(typeof(HubspotPrivateAccessTokenFilterAttribute))]
    [Route("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] ContactDetail contact)
    {
        var client = _httpClientFactory.CreateClient(Constants.HubspotClient);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PrivateAccessToken);

        var response = await client.PatchAsync($"objects/contacts/{id}", 
            new StringContent(JsonSerializer.Serialize(contact), Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to update contact with id: {id} from HubSpot API. {StatusCode} {ReasonPhrase}", id, response.StatusCode, response.ReasonPhrase);
            return BadRequest(response.ReasonPhrase);
        }

        return Ok();
    }

    /// <summary>
    /// Delete contact.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>No Content, Unauthorized or BadRequest status codes.</returns>
    [HttpDelete]
    [ServiceFilter(typeof(HubspotPrivateAccessTokenFilterAttribute))]
    [Route("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var client = _httpClientFactory.CreateClient(Constants.HubspotClient);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PrivateAccessToken);

        var response = await client.DeleteAsync($"objects/contacts/{id}");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to delete contact with id: {id} from HubSpot API. {StatusCode} {ReasonPhrase}", id, response.StatusCode, response.ReasonPhrase);
            return BadRequest(response.ReasonPhrase);
        }

        return NoContent();
    }
}
