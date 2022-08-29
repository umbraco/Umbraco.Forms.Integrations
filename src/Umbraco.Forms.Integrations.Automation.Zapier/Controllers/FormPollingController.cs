using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Options;

using Umbraco.Forms.Core.Data.Storage;
using Umbraco.Forms.Integrations.Automation.Zapier.Extensions;
using Umbraco.Forms.Integrations.Automation.Zapier.Helpers;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;
using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;

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

        public FormPollingController(IOptions<ZapierSettings> options, ZapierFormService zapierFormService, IRecordStorage recordStorage, 
            UmbUrlHelper umbUrlHelper,
            IUserValidationService userValidationService)
            : base(options, userValidationService)
        {
            _zapierFormService = zapierFormService;

            _recordStorage = recordStorage;

            _umbUrlHelper = umbUrlHelper;
        }

        public List<Dictionary<string, string>> GetFormPropertiesById(string id)
        {
            if (!IsAccessValid()) return new List<Dictionary<string, string>>();

            var form = _zapierFormService.GetById(id);

            var latestFormRecord = _recordStorage.GetAllRecords(form)
                .OrderByDescending(p => p.Created)
                .FirstOrDefault();

            return form != null && latestFormRecord != null
                ? new List<Dictionary<string, string>>
                    {form.ToFormDictionary(latestFormRecord, _umbUrlHelper.GetPageUrl(latestFormRecord.UmbracoPageId))}
                : new List<Dictionary<string, string>>();
        }
    }
}
