using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services
{
    public class ContactService : IContactService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger<ContactService> _logger;

        [Obsolete("Use the constructor that takes an ILogger. Scheduled for removal in Umbraco 21.")]
        public ContactService(IHttpClientFactory httpClientFactory)
            : this(httpClientFactory, NullLogger<ContactService>.Instance)
        {
        }

        [ActivatorUtilitiesConstructor]
        public ContactService(IHttpClientFactory httpClientFactory, ILogger<ContactService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<string> CreateOrUpdate(ContactDetailDto contactRequestDto, bool update = false)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClient);

            var requestContent = new StringContent(JsonSerializer.Serialize(contactRequestDto));

            var response = update
               ? await client.PutAsync($"contacts/{contactRequestDto.Contact.Id}", requestContent)
               : await client.PostAsync("contacts", requestContent);

            if(response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();

                return JsonObject.Parse(result)["contact"]["id"].ToString();
            }

            return string.Empty;
        }

        public async Task<ContactCollectionResponseDto?> Get(string email)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClient);

            // The email is a query value, so it isn't part of the logged endpoint name.
            return await ApiResponseReader.SendAsync<ContactCollectionResponseDto>(
                client, "contacts", () => client.GetAsync($"contacts?email={email}"), _logger);
        }

        public async Task<CustomFieldCollectionResponseDto?> GetCustomFields()
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClient);

            return await ApiResponseReader.SendAsync<CustomFieldCollectionResponseDto>(
                client, "fields", () => client.GetAsync("fields"), _logger);
        }
    }
}
