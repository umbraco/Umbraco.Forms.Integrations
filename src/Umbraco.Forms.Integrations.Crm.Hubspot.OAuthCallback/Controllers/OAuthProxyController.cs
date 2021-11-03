using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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

        public OAuthProxyController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _appSettings = appSettings.Value;
        }

        [HttpPost]
        [Route("/oauth/v1/token")]
        public async Task ProxyTokenRequest()
        {
            var httpClient = _httpClientFactory.CreateClient("HubspotToken");

            var response = await httpClient.PostAsync("oauth/v1/token", GetContent(Request.Form));
            var content = await response.Content.ReadAsStringAsync();

            Response.StatusCode = (int)response.StatusCode;

            Response.ContentType = response.Content.Headers.ContentType?.ToString();
            Response.ContentLength = response.Content.Headers.ContentLength;

            await Response.WriteAsync(content);
        }

        private HttpContent GetContent(IFormCollection form)
        {
            var dictionary = form.ToDictionary(x => x.Key, x => x.Value.ToString());
            dictionary.Add("client_secret", _appSettings.ClientSecret);
            return new FormUrlEncodedContent(dictionary);
        }
    }
}
