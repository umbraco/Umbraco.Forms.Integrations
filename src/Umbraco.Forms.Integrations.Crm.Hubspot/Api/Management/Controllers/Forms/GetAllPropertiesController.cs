using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models.Responses;
using Umbraco.Forms.Integrations.Crm.Hubspot.Services;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Api.Management.Controllers.Forms
{
    public class GetAllPropertiesController : ContactControllerBase
    {
        public GetAllPropertiesController(IContactService contactService) : base(contactService)
        {
        }

        [HttpGet("properties")]
        [ProducesResponseType(typeof(IEnumerable<Property>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll() => Ok(await ContactService.GetContactPropertiesAsync());
    }
}
