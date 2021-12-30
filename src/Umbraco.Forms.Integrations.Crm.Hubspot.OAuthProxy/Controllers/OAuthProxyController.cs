using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Umbraco.Cms.Integrations.Authorization.Core.Models.Enums;
using Umbraco.Cms.Integrations.Authorization.Core.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.Hubspot.OAuthProxy.Configuration;
using Umbraco.Forms.Integrations.Crm.Hubspot.OAuthProxy.Factories;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.OAuthProxy.Controllers
{
    [ApiController]
    public class OAuthProxyController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _appSettings;

        private readonly AuthorizationServiceFactory _authorizationServiceFactory;

        public OAuthProxyController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings,
            AuthorizationServiceFactory authorizationServiceFactory)
        {
            _httpClientFactory = httpClientFactory;
            _appSettings = appSettings.Value;

            _authorizationServiceFactory = authorizationServiceFactory;
        }

        [HttpPost]
        [Route("/oauth/v1/token")]
        public async Task ProxyTokenRequest()
        {
            if (Request.Headers.TryGetValue("service_type", out var serviceType))
            {
                try
                {
                    var serviceTypeParsed =
                        (ServiceType.ServiceTypeEnum) Enum.Parse(typeof(ServiceType.ServiceTypeEnum), serviceType);

                    var service = _authorizationServiceFactory.Create(serviceTypeParsed);

                    var content = service.GetContent(Request.Form);

                    var response = await service.ProcessAsync(content);

                    await Response.WriteAsync(response.Content);
                }
                catch (ArgumentException ex)
                {
                    Response.StatusCode = StatusCodes.Status500InternalServerError;

                    await Response.WriteAsync(ex.Message);
                }
                catch (NotImplementedException ex)
                {
                    Response.StatusCode = StatusCodes.Status500InternalServerError;

                    await Response.WriteAsync(ex.Message);
                }
            }
            else
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;

                await Response.WriteAsync("Service Type header is missing.");
            }
        }
    }
}
