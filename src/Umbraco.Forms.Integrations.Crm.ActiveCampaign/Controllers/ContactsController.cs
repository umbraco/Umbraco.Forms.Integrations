using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Configuration;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Controllers
{
    [PluginController("UmbracoFormsIntegrationsCrmActiveCampaign")]
    public class ContactsController : UmbracoAuthorizedApiController
    {
        private readonly ActiveCampaignSettings _settings;

        private readonly IContactService _contactService;

        public ContactsController(IOptions<ActiveCampaignSettings> options, IContactService contactService)
        {
            _settings = options.Value;

            _contactService = contactService;
        }

        [HttpGet]
        public IActionResult CheckApiAccess() => new JsonResult(new ApiAccessDto(_settings.BaseUrl, _settings.ApiKey));

        [HttpGet]
        public IActionResult GetContactFields() => new JsonResult(_settings.ContactFields);

        [HttpGet]
        public IActionResult GetCustomFields() =>  
            new JsonResult(_contactService.GetCustomFields().ConfigureAwait(false).GetAwaiter().GetResult());
    }
}
