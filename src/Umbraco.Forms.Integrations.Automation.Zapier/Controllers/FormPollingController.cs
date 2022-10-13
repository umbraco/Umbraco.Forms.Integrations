using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Forms.Core.Data.Storage;
using Umbraco.Forms.Core.Services;
using Umbraco.Forms.Integrations.Automation.Zapier.Extensions;
using Umbraco.Forms.Integrations.Automation.Zapier.Helpers;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

#if NETCOREAPP
using Microsoft.Extensions.Options;

using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;
#endif

namespace Umbraco.Forms.Integrations.Automation.Zapier.Controllers
{
    /// <summary>
    /// When a Zapier user creates a "New Form Submitted" trigger, they are authenticated, they select a form, and the API provides an output json with the
    /// structure of the selected form.
    /// </summary>
    public class FormPollingController : ZapierFormAuthorizedApiController
    {
        private readonly ZapierFormService _zapierFormService;

        private readonly IRecordStorage _recordStorage;

        private readonly UmbUrlHelper _umbUrlHelper;

#if NETCOREAPP
        public FormPollingController(IOptions<ZapierSettings> options, ZapierFormService zapierFormService, IRecordStorage recordStorage, 
            UmbUrlHelper umbUrlHelper,
            IUserValidationService userValidationService)
            : base(options, userValidationService)
#else
        public FormPollingController(ZapierFormService zapierFormService, IRecordStorage recordStorage,
            UmbUrlHelper umbUrlHelper,
            IUserValidationService userValidationService)
            : base(userValidationService)
#endif
        {
            _zapierFormService = zapierFormService;

            _recordStorage = recordStorage;

            _umbUrlHelper = umbUrlHelper;
        }

        public List<Dictionary<string, string>> GetFormPropertiesById(string id)
        {
            if (!IsAccessValid()) return new List<Dictionary<string, string>>();

            var form = _zapierFormService.GetById(id);

            if(form == null) return new List<Dictionary<string, string>>();

            return new List<Dictionary<string, string>> { form.ToEmptyFormDictionary() };
        }
    }
}
