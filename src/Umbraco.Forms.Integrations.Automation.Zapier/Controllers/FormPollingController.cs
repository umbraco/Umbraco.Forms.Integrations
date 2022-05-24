using System;
using System.Collections.Generic;
using System.Linq;

using Umbraco.Cms.Integrations.Automation.Zapier.Models.Dtos;

using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;
using Umbraco.Forms.Integrations.Automation.Zapier.Extensions;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

#if NETCOREAPP
using Microsoft.Extensions.Options;
#else
using System.Configuration;
#endif

namespace Umbraco.Forms.Integrations.Automation.Zapier.Controllers
{
    /// <summary>
    /// When a Zapier user creates a "New Form Submitted" trigger, he is authenticated, then selects a form, the API provides an output json with the
    /// structure of the selected form.
    /// </summary>
    public class FormPollingController : ZapierFormAuthorizedApiController
    {
        private readonly ZapierSettings Options;

        private readonly ZapierFormService _zapierFormService;

        private readonly IUserValidationService _userValidationService;

#if NETCOREAPP
        public FormPollingController(IOptions<ZapierSettings> options, ZapierFormService zapierFormService, IUserValidationService userValidationService)
            : base(options, userValidationService)
#else
        public FormPollingController(ZapierFormService zapierFormService, IUserValidationService userValidationService)
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
