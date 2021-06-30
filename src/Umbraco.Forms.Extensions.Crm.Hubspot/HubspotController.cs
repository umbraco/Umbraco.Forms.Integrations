using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Forms.Core;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Umbraco.Forms.Extensions.Crm.Hubspot
{
    [PluginController("FormsExtensions")]
    public class HubspotController : UmbracoAuthorizedJsonController
    {
        static readonly HttpClient client = new HttpClient();
        
        private readonly IFacadeConfiguration _configuration;
        
        public HubspotController(IFacadeConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// ~/Umbraco/[YourAreaName]/[YourControllerName]
        /// ~/Umbraco/FormsExtensions/Hubspot/GetAllProperties
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Property>> GetAllProperties()
        {
            var properties = new List<Property>();
            var apiKey = _configuration.GetSetting("HubSpotApiKey");
            var url = $"https://api.hubapi.com/crm/v3/properties/contacts?hapikey={apiKey}";
            var propertiesApiUrl = new Uri(url);

            // Map fields we have in settings to these fields/properties in Hubspot
            var propertiesResponse = client.GetAsync(propertiesApiUrl).Result;
            if (propertiesResponse.IsSuccessStatusCode == false)
            {
                // Log error
                Current.Logger.Error<HubspotWorkflow>("Failed to fetch contact properties from HubSpot API for mapping. {StatusCode} {ReasonPhrase}", propertiesResponse.StatusCode, propertiesResponse.ReasonPhrase);
            }

            // Map Properties back to our simpler object
            // Don't need all the fields in the response
            var rawResult = await propertiesResponse.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<PropertyResult>(rawResult);
            properties.AddRange(json.Results);
            return properties.OrderBy(x => x.Label);
        }

        public class PropertyResult
        {
            [JsonProperty(PropertyName = "results")]
            public List<Property> Results { get; set; }
        }

        public class Property
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "label")]
            public string Label { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }
        }
    }
}
