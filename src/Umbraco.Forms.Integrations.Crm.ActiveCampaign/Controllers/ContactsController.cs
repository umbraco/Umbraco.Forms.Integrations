using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Configuration;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Controllers
{
    [PluginController("UmbracoFormsIntegrationsCrmActiveCampaign")]
    public class ContactsController : UmbracoAuthorizedApiController
    {
        private readonly ActiveCampaignSettings _settings;

        public ContactsController(IOptions<ActiveCampaignSettings> options)
        {
            _settings = options.Value;
        }

        [HttpGet]
        public IActionResult CheckApiAccess() => new JsonResult(new ApiAccessDto(_settings.BaseUrl, _settings.ApiKey));

        [HttpGet]
        public IActionResult GetContactFields() => new JsonResult(_settings.ContactFields);
    }
}
