using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models.Responses;
using Umbraco.Forms.Integrations.Crm.Hubspot.Services;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Api.Management.Controllers.Contacts
{
    [ApiVersion("1.0")]
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
