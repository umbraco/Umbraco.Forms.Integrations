using System.Collections.Generic;
using System.Linq;

using Umbraco.Forms.Integrations.Automation.Zapier.Models.Dtos;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

#if NETCOREAPP
using Microsoft.Extensions.Options;

using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;
#endif

namespace Umbraco.Forms.Integrations.Automation.Zapier.Controllers
{
    /// <summary>
    /// When a Zapier user creates a new "New Form Submitted" trigger, the API is used to provide him with the list of forms.
    /// </summary>
    public class FormController : ZapierFormAuthorizedApiController
    {
        private readonly ZapierFormService _zapierFormService;

#if NETCOREAPP
        public FormController(IOptions<ZapierSettings> options, IUserValidationService userValidationService, ZapierFormService zapierFormService)
            : base(options, userValidationService)
#else
        public FormController(ZapierFormService zapierFormService, IUserValidationService userValidationService)
            : base(userValidationService)
#endif
        {
            _zapierFormService = zapierFormService;
        }

        public IEnumerable<FormDto> GetForms()
        {
            if (!IsUserValid()) return Enumerable.Empty<FormDto>();

            return _zapierFormService.GetAll();
        }

    }
}
