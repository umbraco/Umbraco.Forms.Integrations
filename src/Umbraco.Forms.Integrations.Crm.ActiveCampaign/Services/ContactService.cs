using System.Text.Json;

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

        public async Task<bool> CreateOrUpdate(ContactRequestDto contactRequestDto, bool update = false)
        {
            var client = _httpClientFactory.CreateClient(Constants.ContactsHttpClient);

            var request = new HttpRequestMessage
            {
                Method = update ? HttpMethod.Put : HttpMethod.Post,
                RequestUri = update
                    ? new Uri($"{client.BaseAddress}/{contactRequestDto.Contact.Id}") 
                    : client.BaseAddress,
                Content = new StringContent(JsonSerializer.Serialize(contactRequestDto))
            };

            var response = await client.SendAsync(request);

            var result = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode;
        }

        public async Task<ContactCollectionResponseDto> Get(string email)
        {
            var client = _httpClientFactory.CreateClient(Constants.ContactsHttpClient);

            var response = await client.SendAsync(
                new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{client.BaseAddress}?email={email}")
                });

            var content = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ContactCollectionResponseDto>(content);
        }
    }
}
