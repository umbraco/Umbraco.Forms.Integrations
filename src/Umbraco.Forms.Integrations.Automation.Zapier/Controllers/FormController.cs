using System.Collections.Generic;
using System.Linq;

using Umbraco.Cms.Integrations.Automation.Zapier.Models.Dtos;
using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

#if NETCOREAPP
using Microsoft.Extensions.Options;

using Umbraco.Cms.Web.Common.Controllers;
#else
using System.Configuration;
#endif

namespace Umbraco.Forms.Integrations.Automation.Zapier.Controllers
{
    /// <summary>
    /// When a Zapier user creates a new "New Form Submitted" trigger, the API is used to provide him with the list of forms.
    /// </summary>
    public class FormController : ZapierFormAuthorizedApiController
    {
        private readonly ZapierSettings Options;

        private readonly IUserValidationService _userValidationService;

        private readonly ZapierFormService _zapierFormService;

#if NETCOREAPP
        public FormController(IOptions<ZapierSettings> options, IUserValidationService userValidationService, ZapierFormService zapierFormService)
            : base(options, userValidationService)
#else
        public FormController(ZapierFormService zapierFormService, IUserValidationService userValidationService)
            : base(userValidationService)
#endif
        {
#if NETCOREAPP
            Options = options.Value;
#else
            Options = new ZapierSettings(ConfigurationManager.AppSettings);
#endif

            _zapierFormService = zapierFormService;

            _userValidationService = userValidationService;
        }

        public IEnumerable<FormDto> GetForms()
        {
            if (!IsUserValid()) return Enumerable.Empty<FormDto>();

            return _zapierFormService.GetAll();
        }

    }
}
