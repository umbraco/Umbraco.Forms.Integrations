using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Forms.Core.Services;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Api.Management.Controllers.Forms
{
    [ApiVersion("1.0")]
    public class GetFormFieldsController : FormControllerBase
    {
        public GetFormFieldsController(IFormService formService) : base(formService)
        {
        }

        [HttpGet("fields")]
        [ProducesResponseType(typeof(List<ActiveCampaignFormFieldDto>), StatusCodes.Status200OK)]
        public IActionResult GetFormFields(string formId)
        {
            List<ActiveCampaignFormFieldDto> formFields = new List<ActiveCampaignFormFieldDto>();
            var result = FormService.Get(new Guid(formId));
            if (result != null)
            {
                formFields = result.AllFields.Select(s => new ActiveCampaignFormFieldDto { Caption = s.Caption, Id = s.Id }).ToList();
            }
            return Ok(formFields);
        }
    }
}
