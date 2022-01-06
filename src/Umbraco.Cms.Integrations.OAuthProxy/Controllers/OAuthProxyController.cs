using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Integrations.OAuthProxy.Configuration;

namespace Umbraco.Cms.Integrations.OAuthProxy.Controllers
{
    [ApiController]
    public class OAuthProxyController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _appSettings;

        public const string ServiceNameHeaderKey = "service_name";

        /// <summary>
        /// Integrated services with their token URIs
        /// </summary>
        private static Dictionary<string, string> ValidServices = new Dictionary<string, string>
        {
            { "Hubspot", "oauth/v1/token" }, { "Semrush", "oauth2/access_token" }
        };

        //private static List<string> ValidServiceNames = new List<string> {"Hubspot", "Semrush"};

        public OAuthProxyController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _appSettings = appSettings.Value;
        }

        [HttpPost]
        [Route("/oauth/v1/token")]
        public async Task ProxyTokenRequest()
        {
            if (!Request.Headers.TryGetValue(ServiceNameHeaderKey, out var serviceName))
            {
                serviceName = "Hubspot";
            }

            if (!ValidServices.ContainsKey(serviceName))
            {
                throw 
                    new InvalidOperationException($"Provided {ServiceNameHeaderKey} header value of {serviceName} is not supported");
            }

            var httpClient = _httpClientFactory.CreateClient($"{serviceName}Token");

            var response =
                await httpClient.PostAsync(ValidServices[serviceName], GetContent(Request.Form, serviceName));
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
