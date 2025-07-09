using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Configuration;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Api.Management.Controllers.Contacts
{
    [ApiVersion("1.0")]
    public class CheckApiAccessController : ContactControllerBase
    {
        public CheckApiAccessController(IOptions<ActiveCampaignSettings> options, IContactService contactService) : base(options, contactService)
        {
        }

        [HttpGet("api-access")]
        [ProducesResponseType(typeof(ApiAccessDto), StatusCodes.Status200OK)]
        public IActionResult CheckApiAccess() => Ok(new ApiAccessDto(_settings.BaseUrl, _settings.ApiKey));
    }
}
