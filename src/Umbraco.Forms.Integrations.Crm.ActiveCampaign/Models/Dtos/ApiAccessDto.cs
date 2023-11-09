using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

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
        ? _baseUrl.Substring(0, _baseUrl.IndexOf(".")).Replace("https://", string.Empty)
        : string.Empty;

    public bool IsApiConfigurationValid => !string.IsNullOrEmpty(_baseUrl) && !string.IsNullOrEmpty(_apiKey);
}
