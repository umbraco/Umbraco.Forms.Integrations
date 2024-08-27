using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Forms.Integrations.Automation.Zapier.Models.Dtos;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Api.Management.Controllers
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.GroupName)]
    public class GetFormsController : FormsControllerBase
    {
        public GetFormsController(IUserValidationService userValidationService, ZapierFormService zapierFormService) 
            : base(userValidationService, zapierFormService)
        {
            
        }

        [HttpGet("forms")]
        [ProducesResponseType(typeof(IEnumerable<FormDto>), StatusCodes.Status200OK)]
        public IActionResult GetForms()
        {
            if (!IsAccessValid()) return Ok(Enumerable.Empty<FormDto>());

            return Ok(ZapierFormService.GetAll());
        }
    }
}
