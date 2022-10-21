using System.Text.Json;

using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services
{
    public class AccountService : IAccountService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<AccountCollectionResponseDto> Get()
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClient);

            var response = await client.GetAsync("accounts");

            var content = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<AccountCollectionResponseDto>(content);
        }

        public async Task<AccountContactRequestDto> CreateAssociation(int accountId, int contactId)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClient);

            var accountContact = new AccountContactRequestDto
            {
                AccountContact = new AccountContactDto
                {
                    AccountId = accountId,
                    ContactId = contactId
                }
            };

            var response = await client.PostAsync("accountContacts", new StringContent(JsonSerializer.Serialize(accountContact)));

            var content = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<AccountContactRequestDto>(content);
        }
    }
}
