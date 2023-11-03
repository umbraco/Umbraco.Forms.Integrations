using System.Collections.Generic;
using System.Linq;

using Umbraco.Forms.Integrations.Automation.Zapier.Models.Dtos;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Controllers
{
    /// <summary>
    /// When a Zapier user creates a new "New Form Submitted" trigger, the API is used to provide them with the list of forms.
    /// </summary>
    public class FormController : ZapierFormAuthorizedApiController
    {
        private readonly ZapierFormService _zapierFormService;

        public FormController(
            IUserValidationService userValidationService, ZapierFormService zapierFormService)
            : base(userValidationService) => _zapierFormService = zapierFormService;

        public IEnumerable<FormDto> GetForms()
        {
            if (!IsAccessValid()) return Enumerable.Empty<FormDto>();

            return _zapierFormService.GetAll();
        }

    }
}
