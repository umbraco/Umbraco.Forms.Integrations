using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Integrations.OAuthProxy.Configuration;

namespace Umbraco.Cms.Integrations.OAuthProxy.Controllers
{
    [ApiController]
    public class OAuthProxyController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _appSettings;

        public const string ServiceNameHeaderKey = "service_name";
        public const string ServiceAddressReplacePrefixHeaderKey = "service_address_";

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

            if (!ServiceConfiguration.ServiceProviders.ContainsKey(serviceName))
            {
                throw 
                    new InvalidOperationException($"Provided {ServiceNameHeaderKey} header value of {serviceName} is not supported");
            }

            var httpClient = _httpClientFactory.CreateClient($"{serviceName}Token");

            var x = GetContent(Request.Form, serviceName);

            var response =
                await httpClient.PostAsync(ServiceConfiguration.ServiceProviders[serviceName], GetContent(Request.Form, serviceName));
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
