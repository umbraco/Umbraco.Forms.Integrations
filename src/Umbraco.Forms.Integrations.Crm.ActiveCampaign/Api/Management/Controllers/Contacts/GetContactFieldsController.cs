using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Configuration;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Api.Management.Controllers.Contacts
{
    public class GetContactFieldsController : ContactControllerBase
    {
        public GetContactFieldsController(IOptions<ActiveCampaignSettings> options, IContactService contactService) : base(options, contactService)
        {
        }

        [HttpGet("fields")]
        [ProducesResponseType(typeof(List<ContactFieldSettings>), StatusCodes.Status200OK)]
        public IActionResult GetContactFields() => Ok(new JsonResult(_settings.ContactFields));
    }
}
