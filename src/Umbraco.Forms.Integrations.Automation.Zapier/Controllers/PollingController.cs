using System;
using System.Collections.Generic;

using System.Linq;
using Umbraco.Forms.Core.Data.Storage;
using Umbraco.Forms.Core.Providers.Models;
using Umbraco.Forms.Core.Services;
using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

#if NETCOREAPP
using Microsoft.Extensions.Options;

using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Core.Services;
#else
using System.Configuration;

using Umbraco.Web.WebApi;
using Umbraco.Core.Services;
#endif

namespace Umbraco.Forms.Integrations.Automation.Zapier.Controllers
{
    public class PollingController : UmbracoApiController
    {
        private readonly ZapierSettings Options;

        private readonly IFormService _formService;

        private readonly IUserValidationService _userValidationService;

#if NETCOREAPP
        private readonly IWorkflowService _workflowService;

        public PollingController(IOptions<ZapierSettings> options, IFormService formService, IWorkflowService workflowService,
            IRecordReaderService recordReaderService, IUserValidationService userValidationService)
#else
        private readonly IWorkflowServices _workflowServices;

        public PollingController(IFormService formService, IWorkflowServices workflowServices, 
            IRecordReaderService recordReaderService, IUserValidationService userValidationService)
#endif
        {
#if NETCOREAPP
            Options = options.Value;

            _workflowService = workflowService;
#else
            Options = new ZapierSettings(ConfigurationManager.AppSettings);

            _workflowServices = workflowServices;
#endif

            _formService = formService;

            _userValidationService = userValidationService;
        }

        public List<Dictionary<string, string>> GetFormsData()
        {
            var formsData = new List<Dictionary<string, string>>();

            string username = string.Empty;
            string password = string.Empty;

#if NETCOREAPP
            if (Request.Headers.TryGetValue(Constants.ZapierAppConfiguration.UsernameHeaderKey,
                    out var usernameValues))
                username = usernameValues.First();
            if (Request.Headers.TryGetValue(Constants.ZapierAppConfiguration.PasswordHeaderKey,
                    out var passwordValues))
                password = passwordValues.First();
#else
            if (Request.Headers.TryGetValues(Constants.ZapierAppConfiguration.UsernameHeaderKey,
                    out var usernameValues))
                username = usernameValues.First();
            if (Request.Headers.TryGetValues(Constants.ZapierAppConfiguration.PasswordHeaderKey,
                    out var passwordValues))
                password = passwordValues.First();
#endif

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return null;

            var isAuthorized = _userValidationService.Validate(username, password, Options.UserGroup).GetAwaiter().GetResult();
            if (!isAuthorized) return null;

            // 1. get forms
            var forms = _formService.Get();
            foreach (var form in forms)
            {
#if NETCOREAPP
                var hasZapierWorkflow = _workflowService.Get(form).Any(p => p.WorkflowTypeId == new Guid(Constants.ZapierWorkflowTypeId));
#else
                var hasZapierWorkflow = _workflowServices.Get(form)
                    .Any(p => p.WorkflowTypeId == new Guid(Constants.ZapierWorkflowTypeId));
#endif

                if (hasZapierWorkflow)
                    formsData.Add(new Dictionary<string, string>
                    {
                        { Enum.GetName(typeof(StandardField), StandardField.FormId), form.Id.ToString() },
                        { Enum.GetName(typeof(StandardField), StandardField.FormName), form.Name },
                        { Enum.GetName(typeof(StandardField), StandardField.PageUrl), "/" },
                        { Enum.GetName(typeof(StandardField), StandardField.SubmissionDate), DateTime.Now.ToString() }
                    });
            }

            return formsData;
        }

    }
}
