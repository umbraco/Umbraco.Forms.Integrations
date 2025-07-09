using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Configuration;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Api.Management.Controllers.Contacts
{
    [ApiVersion("1.0")]
    public class GetContactFieldsController : ContactControllerBase
    {
        public GetContactFieldsController(IOptions<ActiveCampaignSettings> options, IContactService contactService) : base(options, contactService)
        {
        }

        [HttpGet("fields")]
        [ProducesResponseType(typeof(List<ContactFieldSettings>), StatusCodes.Status200OK)]
        public IActionResult GetContactFields() => Ok(_settings.ContactFields);
    }
}
