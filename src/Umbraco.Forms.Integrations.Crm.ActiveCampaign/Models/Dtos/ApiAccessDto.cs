using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Configuration;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos
{
    public class ApiAccessDto
    {
        private readonly string _baseUrl;

        private readonly string _apiKey;

        public ApiAccessDto(string baseUrl, string apiKey)
        {
            _baseUrl = baseUrl;
            _apiKey = apiKey;
        }

        public string Account => IsApiConfigurationValid
            && ApiConfiguration.TryGetApiBaseAddress(_baseUrl, out var apiBaseAddress)
            ? apiBaseAddress!.Host.Split('.')[0]
            : string.Empty;

        public bool IsApiConfigurationValid => ApiConfiguration.IsValid(_baseUrl, _apiKey);
    }
}
