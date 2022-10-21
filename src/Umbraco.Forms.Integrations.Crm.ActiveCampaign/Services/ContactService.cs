using System.Text.Json;
using System.Text.Json.Nodes;

using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services
{
    public class ContactService : IContactService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ContactService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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

        public async Task<ContactCollectionResponseDto> Get(string email)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClient);

            var response = await client.GetAsync($"contacts?email={email}");

            var content = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ContactCollectionResponseDto>(content);
        }

        public async Task<CustomFieldCollectionResponseDto> GetCustomFields()
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClient);

            var response = await client.GetAsync("fields");

            var content = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<CustomFieldCollectionResponseDto>(content);
        }
    }
}
