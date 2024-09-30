using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.Hubspot.Services;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Api.Management.Controllers.Contacts
{
    public class AuthorizeController : ContactControllerBase
    {
        public AuthorizeController(IContactService contactService) : base(contactService)
        {
        }

        [HttpPost("authorize")]
        [ProducesResponseType(typeof(AuthorizationResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Authorize([FromBody]AuthorizationRequest request) => Ok(await ContactService.AuthorizeAsync(request.Code));
    }
}
