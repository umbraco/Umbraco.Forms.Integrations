using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Integrations.Authorization.Core.Models.Enums;
using Umbraco.Cms.Integrations.Authorization.Core.Interfaces;
using Umbraco.Cms.Integrations.Authorization.Core.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.Hubspot.OAuthProxy.Configuration;
using Umbraco.Forms.Integrations.Crm.Hubspot.OAuthProxy.Services;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.OAuthProxy.Factories
{
    public class AuthorizationServiceFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IConfiguration _configuration;

        public AuthorizationServiceFactory(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;

            _configuration = configuration;
        }

        public IAuthorizationService Create(ServiceType.ServiceTypeEnum serviceType)
        {
            switch (serviceType)
            {
                case ServiceType.ServiceTypeEnum.Hubspot:
                    return new HubspotService(_httpClientFactory, _configuration);
                case ServiceType.ServiceTypeEnum.Semrush:
                    return new SemrushService(_httpClientFactory, _configuration);
                default: throw new NotImplementedException("Service Type not implemented.");
            }
        }
    }
}
