using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Threading.Tasks;

using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models.Responses;
using Umbraco.Forms.Integrations.Crm.Hubspot.Services;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Controllers
{
    [PluginController("UmbracoFormsIntegrationsCrmHubspot")]
    public class HubspotController : UmbracoAuthorizedJsonController
    {
        private readonly IContactService _contactService;

        public HubspotController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpGet]
        public string IsAuthorizationConfigured() => _contactService.IsAuthorizationConfigured().ToString();

        [HttpGet]
        public string GetAuthenticationUrl() => _contactService.GetAuthenticationUrl();

        [HttpPost]
        public async Task<AuthorizationResult> Authorize([FromBody] AuthorizationRequest request) => await _contactService.AuthorizeAsync(request.Code);

        [HttpPost]
        public AuthorizationResult Deauthorize() => _contactService.Deauthorize();

        [HttpGet]
        public async Task<IEnumerable<Property>> GetAllProperties() => await _contactService.GetContactPropertiesAsync();
    }
}
