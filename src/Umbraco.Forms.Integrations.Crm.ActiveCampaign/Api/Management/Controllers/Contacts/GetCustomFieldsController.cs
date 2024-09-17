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
    public class GetCustomFieldsController : ContactControllerBase
    {
        public GetCustomFieldsController(IOptions<ActiveCampaignSettings> options, IContactService contactService) : base(options, contactService)
        {
        }

        [HttpGet("custom")]
        [ProducesResponseType(typeof(CustomFieldCollectionResponseDto), StatusCodes.Status200OK)]
        public IActionResult GetCustomFields() =>
            Ok(new JsonResult(_contactService.GetCustomFields().ConfigureAwait(false).GetAwaiter().GetResult()));
    }
}
