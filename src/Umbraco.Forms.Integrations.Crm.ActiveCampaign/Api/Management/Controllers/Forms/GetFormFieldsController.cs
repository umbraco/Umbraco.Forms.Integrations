using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Actions;
using Umbraco.Forms.Core.Models;
using Umbraco.Forms.Core.Services;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Configuration;


namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Api.Management.Controllers.Forms
{
    public class GetFormFieldsController : FormControllerBase
    {
        public GetFormFieldsController(IFormService formService) : base(formService)
        {
        }

        [HttpGet("fields")]
        [ProducesResponseType(typeof(List<Field>), StatusCodes.Status200OK)]
        public IActionResult GetFormFields(string formId)
        {
            List<Field> formFields = new List<Field>();
            var result = FormService.Get(new Guid(formId));
            if (result != null)
            {
                formFields = result.AllFields;
            }
            return Ok(formFields);
        }
    }
}
