using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Forms.Integrations.Crm.Hubspot.Services;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Api.Management.Controllers.Contacts
{
    public class IsAuthorizationConfiguredController : ContactControllerBase
    {
        public IsAuthorizationConfiguredController(IContactService contactService) : base(contactService)
        {
        }

        [HttpGet("auth/configured")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult IsAuthorizationConfigured() => Ok(ContactService.IsAuthorizationConfigured().ToString());
    }
}
