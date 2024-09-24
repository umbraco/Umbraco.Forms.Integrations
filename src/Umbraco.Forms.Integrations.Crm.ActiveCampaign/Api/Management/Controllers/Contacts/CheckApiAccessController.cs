using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Configuration;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Api.Management.Controllers.Contacts
{
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
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
