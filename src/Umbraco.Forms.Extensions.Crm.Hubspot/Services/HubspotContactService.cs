using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Extensions.Crm.Hubspot.Models;
using Umbraco.Forms.Extensions.Crm.Hubspot.Models.Responses;

namespace Umbraco.Forms.Extensions.Crm.Hubspot.Services
{
    public class HubspotContactService : IContactService
    {
        static readonly HttpClient client = new HttpClient();

        private readonly IFacadeConfiguration _configuration;
        private readonly ILogger _logger;

        public HubspotContactService(IFacadeConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<Property>> GetContactProperties()
        {
            if (!TryGetApiKey(out string apiKey))
            {
                _logger.Warn<HubspotContactService>("Failed to fetch contact properties from HubSpot API for mapping as no API Key has been configured.");
                return Enumerable.Empty<Property>();
            }

            var url = ConstructUrl("properties/contacts", apiKey);
            var response = await client.GetAsync(new Uri(url));
            if (response.IsSuccessStatusCode == false)
            {
                _logger.Error<HubspotContactService>("Failed to fetch contact properties from HubSpot API for mapping. {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                return Enumerable.Empty<Property>();
            }

            // Map Properties back to our simpler object
            // Don't need all the fields in the response
            var properties = new List<Property>();
            var rawResult = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<PropertiesResponse>(rawResult);
            properties.AddRange(json.Results);
            return properties.OrderBy(x => x.Label);
        }

        public async Task<CommandResult> PostContact(Record record, List<MappedProperty> fieldMappings)
        {
            if (!TryGetApiKey(out string apiKey))
            {
                _logger.Warn<HubspotContactService>("Failed to fetch contact properties from HubSpot API for mapping as no API Key has been configured.");
                return CommandResult.NotConfigured;
            }

            // Map data from the workflow setting Hubspot fields
            // From the form field values submitted for this form submission
            var postData = new PropertiesRequest();
            foreach (var mapping in fieldMappings)
            {
                var fieldId = mapping.FormField;
                var recordField = record.GetRecordField(new Guid(fieldId));
                if (recordField != null)
                {
                    // TODO: What about different field types in forms & Hubspot that are not simple text ones ?
                    postData.Properties.Add(mapping.HubspotField, recordField.ValuesAsString(false));
                }
                else
                {
                    // There field mapping value could not be found.
                    // Write a warning in the log
                    _logger.Warn<HubspotContactService>("The field mapping with Id, {FieldMappingId}, did not match any record fields. This is probably caused by the record field being marked as sensitive and the workflow has been set not to include sensitive data", mapping.FormField);
                }
            }

            // Serialise dynamic JObject to a string for StringContent to POST to URL
            var objAsJson = JsonConvert.SerializeObject(postData);
            var content = new StringContent(objAsJson, Encoding.UTF8, "application/json");

            // POST data to hubspot
            // https://api.hubapi.com/crm/v3/objects/contacts?hapikey=YOUR_HUBSPOT_API_KEY
            var url = ConstructUrl("objects/contacts", apiKey);
            var response = await client.PostAsync(url, content).ConfigureAwait(false);

            // Depending on POST status fail or mark workflow as completed
            if (response.IsSuccessStatusCode == false)
            {
                _logger.Error<HubspotContactService>("Error submitting a HubSpot contact request ");
                return CommandResult.Failed;
            }

            return CommandResult.Success;
        }

        private bool TryGetApiKey(out string apiKey)
        {
            apiKey = _configuration.GetSetting("HubSpotApiKey");
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.Warn<HubspotContactService>("Failed to fetch contact properties from HubSpot API for mapping as no API Key has been configured.");
                return false;
            }

            return true;
        }

        private string ConstructUrl(string path, string apiKey)
        {
            const string ApiBaseUrl = "https://api.hubapi.com/crm/v3/";
            return $"{ApiBaseUrl}{path}?hapikey={apiKey}";
        }
    }
}
