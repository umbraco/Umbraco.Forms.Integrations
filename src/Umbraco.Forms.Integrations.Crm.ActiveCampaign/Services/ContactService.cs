using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services
{
    public class ContactService : IContactService
    {
        private const int CustomFieldPageSize = 100;

        private const int MaxCustomFieldPages = 50;

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

            var fields = new List<CustomFieldDto>();
            var seenIds = new HashSet<string>();

            for (var page = 0; page < MaxCustomFieldPages; page++)
            {
                var offset = page * CustomFieldPageSize;

                var result = await ApiResponseReader.SendAsync<CustomFieldPage>(
                    client, $"fields (offset {offset})", () => client.GetAsync($"fields?limit={CustomFieldPageSize}&offset={offset}"), _logger);

                if (result is null)
                {
                    return null;
                }

                if (result.Fields is null || result.Fields.Count == 0)
                {
                    break;
                }

                // Skip repeats, in case the order shifts between pages or offset is ignored.
                var added = 0;
                foreach (var field in result.Fields)
                {
                    if (field.Id is null || seenIds.Add(field.Id))
                    {
                        fields.Add(field);
                        added++;
                    }
                }

                if (added == 0)
                {
                    break;
                }

                if (fields.Count >= (result.Meta?.Total ?? 0))
                {
                    break;
                }

                if (page == MaxCustomFieldPages - 1)
                {
                    _logger.LogWarning(
                        "ActiveCampaign custom fields stopped after {Pages} pages ({Count} fields collected); some fields may be missing.",
                        MaxCustomFieldPages, fields.Count);
                }
            }

            return new CustomFieldCollectionResponseDto { Fields = fields };
        }

        // ActiveCampaign pages list endpoints (max 100 per request) and sends meta.total as a string.
        private sealed class CustomFieldPage
        {
            [JsonPropertyName("fields")]
            public List<CustomFieldDto>? Fields { get; set; }

            [JsonPropertyName("meta")]
            public PageMeta? Meta { get; set; }
        }

        private sealed class PageMeta
        {
            [JsonPropertyName("total")]
            [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
            public int Total { get; set; }
        }
    }
}
