using System;
using System.Collections.Generic;
using System.Linq;

using Umbraco.Forms.Integrations.Automation.Zapier.Models.Dtos;
using Umbraco.Forms.Integrations.Automation.Zapier.Extensions;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

#if NETCOREAPP
using Microsoft.Extensions.Options;

using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;
#endif

namespace Umbraco.Forms.Integrations.Automation.Zapier.Controllers
{
    /// <summary>
    /// When a Zapier user creates a "New Form Submitted" trigger, he is authenticated, then selects a form, the API provides an output json with the
    /// structure of the selected form.
    /// </summary>
    public class FormPollingController : ZapierFormAuthorizedApiController
    {
        private readonly ZapierFormService _zapierFormService;

#if NETCOREAPP
        public FormPollingController(IOptions<ZapierSettings> options, ZapierFormService zapierFormService, IUserValidationService userValidationService)
            : base(options, userValidationService)
#else
        public FormPollingController(ZapierFormService zapierFormService, IUserValidationService userValidationService)
            : base(userValidationService)
#endif
        {
            _zapierFormService = zapierFormService;
        }

        [Obsolete("Used only for Umbraco Zapier app v1.0.0. For updated versions use GetFormById")]
        public IEnumerable<FormDto> GetSampleForm()
        {
            if (!IsUserValid()) return null;

            return _zapierFormService.GetAll().Take(1);
        }

        public List<Dictionary<string, string>> GetFormPropertiesById(string id)
        {
            if (!IsUserValid()) return new List<Dictionary<string, string>>();

            var form = _zapierFormService.GetById(id);
            return form != null
                ? new List<Dictionary<string, string>> { form.ToFormDictionary() }
                : new List<Dictionary<string, string>>();
        }
    }
}
