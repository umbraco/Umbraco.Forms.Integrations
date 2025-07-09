using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Forms.Core.Services;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models.Dtos;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Api.Management.Controllers.Forms
{
    [ApiVersion("1.0")]
    public class GetFormFieldsController : FormControllerBase
    {
        public GetFormFieldsController(IFormService formService) : base(formService)
        {
        }

        [HttpGet("fields")]
        [ProducesResponseType(typeof(List<HubspotWorkflowFormFieldDto>), StatusCodes.Status200OK)]
        public IActionResult GetFormFields(string formId)
        {
            List<HubspotWorkflowFormFieldDto> formFields = new List<HubspotWorkflowFormFieldDto>();
            var result = FormService.Get(new Guid(formId));
            if (result != null)
            {
                formFields = result.AllFields.Select(s => new HubspotWorkflowFormFieldDto{ Caption = s.Caption, Id = s.Id }).ToList();
            }
            return Ok(formFields);
        }
    }
}
