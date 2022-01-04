using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Umbraco.Forms.Integrations.Crm.Hubspot.OAuthProxy.Configuration;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.OAuthProxy.Controllers
{
    [ApiController]
    public class OAuthProxyController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _appSettings;

        private static List<string> ValidServiceNames = new List<string> { "Hubspot", "Semrush" };

        public OAuthProxyController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _appSettings = appSettings.Value;
        }

        [HttpPost]
        [Route("/oauth/v1/token")]
        public async Task ProxyTokenRequest()
        {
            const string ServiceNameHeaderKey = "service_name";
            if (Request.Headers.TryGetValue(ServiceNameHeaderKey, out var serviceName) == false)
            {
                // Default to HubSpot client as that was the only one released before the support for multiple
                // authorization services was introduced.
                serviceName = "Hubspot";
            }

            if (ValidServiceNames.Contains(serviceName) == false)
            {
                throw new InvalidOperationException($"Provided {ServiceNameHeaderKey} header value of {serviceName} is not supported.");
            }
            
            var httpClient = _httpClientFactory.CreateClient($"{serviceName}Token");

            var response = await httpClient.PostAsync("oauth/v1/token", GetContent(Request.Form, serviceName));
            var content = await response.Content.ReadAsStringAsync();

            Response.StatusCode = (int)response.StatusCode;

            Response.ContentType = response.Content.Headers.ContentType?.ToString();
            Response.ContentLength = response.Content.Headers.ContentLength;

            await Response.WriteAsync(content);
        }

        private HttpContent GetContent(IFormCollection form, string serviceName)
        {
            var dictionary = form.ToDictionary(x => x.Key, x => x.Value.ToString());
            dictionary.Add("client_secret", GetClientSecret(serviceName));
            return new FormUrlEncodedContent(dictionary);
        }

        private string GetClientSecret(string serviceName) => _appSettings[$"{serviceName}ClientSecret"];
    }
}
