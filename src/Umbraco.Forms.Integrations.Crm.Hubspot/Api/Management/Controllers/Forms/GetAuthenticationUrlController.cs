using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Forms.Integrations.Crm.Hubspot.Services;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Api.Management.Controllers.Forms
{
    public class GetAuthenticationUrlController : ContactControllerBase
    {
        public GetAuthenticationUrlController(IContactService contactService) : base(contactService)
        {
        }

        [HttpGet("auth/url")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public string GetAuthenticationUrl() => ContactService.GetAuthenticationUrl();
    }
}
