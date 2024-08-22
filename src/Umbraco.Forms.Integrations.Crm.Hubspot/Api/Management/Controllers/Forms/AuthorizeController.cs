using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.Hubspot.Services;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Api.Management.Controllers.Forms
{
    public class AuthorizeController : FormsControllerBase
    {
        public AuthorizeController(IContactService contactService) : base(contactService)
        {
        }

        [HttpPost("authorize")]
        [ProducesResponseType(typeof(Task<AuthorizationResult>), StatusCodes.Status200OK)]
        public IActionResult Authorize([FromBody] AuthorizationRequest request) => Ok(ContactService.AuthorizeAsync(request.Code));
    }
}
