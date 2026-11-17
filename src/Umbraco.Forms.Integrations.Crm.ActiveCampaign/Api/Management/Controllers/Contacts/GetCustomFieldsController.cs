using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Configuration;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Api.Management.Controllers.Contacts
{
    [ApiVersion("1.0")]
    public class GetCustomFieldsController : ContactControllerBase
    {
        public GetCustomFieldsController(IOptions<ActiveCampaignSettings> options, IContactService contactService) : base(options, contactService)
        {
        }

        [HttpGet("custom")]
        [ProducesResponseType(typeof(CustomFieldCollectionResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCustomFields()
        {
            var fields = await _contactService.GetCustomFields();

            if (fields is null)
            {
                return Problem(
                    title: "Could not load custom fields from ActiveCampaign",
                    detail: $"Check the BaseUrl and ApiKey under {Constants.SettingsPath} in appsettings.json, and see the site log for the response from ActiveCampaign.",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            return Ok(fields);
        }
    }
}
