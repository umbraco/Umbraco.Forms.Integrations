using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.Hubspot.Services;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Api.Management.Controllers.Contacts
{
    public class DeauthorizeController : ContactControllerBase
    {
        public DeauthorizeController(IContactService contactService) : base(contactService)
        {
        }

        [HttpPost("deauthorize")]
        [ProducesResponseType(typeof(AuthorizationResult), StatusCodes.Status200OK)]
        public IActionResult Deauthorize() => Ok(ContactService.Deauthorize());
    }
}
