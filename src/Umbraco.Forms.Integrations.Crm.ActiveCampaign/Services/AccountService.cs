using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services
{
    public class AccountService : IAccountService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger<AccountService> _logger;

        [Obsolete("Use the constructor that takes an ILogger. Scheduled for removal in Umbraco 21.")]
        public AccountService(IHttpClientFactory httpClientFactory)
            : this(httpClientFactory, NullLogger<AccountService>.Instance)
        {
        }

        [ActivatorUtilitiesConstructor]
        public AccountService(IHttpClientFactory httpClientFactory, ILogger<AccountService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<AccountCollectionResponseDto?> Get()
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClient);

            return await ApiResponseReader.SendAsync<AccountCollectionResponseDto>(
                client, "accounts", () => client.GetAsync("accounts"), _logger);
        }

        public async Task<AccountContactRequestDto?> CreateAssociation(int accountId, int contactId)
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

            return await ApiResponseReader.SendAsync<AccountContactRequestDto>(
                client,
                "accountContacts",
                () => client.PostAsync("accountContacts", new StringContent(JsonSerializer.Serialize(accountContact))),
                _logger);
        }
    }
}
