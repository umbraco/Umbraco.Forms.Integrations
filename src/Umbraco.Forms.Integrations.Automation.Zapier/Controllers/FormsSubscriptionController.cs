using System;
using System.Linq;

using Umbraco.Forms.Core.Services;
using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;
using Umbraco.Forms.Integrations.Automation.Zapier.Models.Dtos;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.Common.Controllers;
#else
using System.Web.Http;
using System.Configuration;
using Umbraco.Forms.Core.Data.Storage;
using Umbraco.Web.WebApi;
#endif

namespace Umbraco.Forms.Integrations.Automation.Zapier.Controllers
{
    public class FormsSubscriptionController : UmbracoApiController
    {
        private readonly ZapierSettings Options;

#if NETCOREAPP
        private readonly ILogger<FormsSubscriptionController> _logger;

        private readonly IWorkflowService _workflowService;

        private readonly IFormService _formService;
#else
        private readonly IWorkflowServices _workflowServices;

        private readonly IFormStorage _formStorage;
#endif

        private readonly IUserValidationService _userValidationService;

#if NETCOREAPP
        public FormsSubscriptionController(IOptions<ZapierSettings> options, ILogger<FormsSubscriptionController> logger, IFormService formService, 
            IWorkflowService workflowService, IUserValidationService userValidationService)
#else
        public FormsSubscriptionController(IWorkflowServices workflowServices, IFormStorage formStorage, IUserValidationService userValidationService)
#endif
        {
#if NETCOREAPP
            Options = options.Value;

            _logger = logger;

            _workflowService = workflowService;

            _formService = formService;
#else
            Options = new ZapierSettings(ConfigurationManager.AppSettings);

            _workflowServices = workflowServices;

            _formStorage = formStorage;
#endif

            _userValidationService = userValidationService;
        }

        [HttpPost]
        public bool UpdatePreferences([FromBody] SubscriptionDto dto)
        {
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

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return false;

            var isAuthorized = _userValidationService.Validate(username, password, Options.UserGroup).GetAwaiter().GetResult();
            if (!isAuthorized) return false;

            if (dto == null) return false;

            try
            {
                // 1. get forms
#if NETCOREAPP
                var forms = _formService.Get();
#else
                var forms = _formStorage.GetAll();
#endif
                foreach (var form in forms)
                {
                    // 2. check if 'Trigger Zapier' workflow exists on the form
#if NETCOREAPP
                    var zapierWorkflows = _workflowService.Get(form)
                        .Where(p => p.WorkflowTypeId == new Guid(Constants.ZapierWorkflowTypeId)).ToList();
#else
                    var zapierWorkflows = _workflowServices.Get(form)
                        .Where(p => p.WorkflowTypeId == new Guid(Constants.ZapierWorkflowTypeId)).ToList();
#endif
                    if (zapierWorkflows.Any())
                    {
                        foreach (var zapierWorkflow in zapierWorkflows)
                        {
                            // 3. check if request hook URL matches with the WebHookUri property of the workflow. If match found, set Active property.
                            if (zapierWorkflow.Settings[nameof(ZapierWorkflow.WebHookUri)] == dto.HookUrl)
                            {
                                zapierWorkflow.Active = dto.Enable;

#if NETCOREAPP
                                _workflowService.Update(zapierWorkflow);
#else
                                _workflowServices.Update(zapierWorkflow);
#endif
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
#if NETCOREAPP
                _logger.LogError(e.Message);
#else
                Logger.Error(typeof(FormsSubscriptionController), e);
#endif
                return false;
            }

            return true;
        }
    }
}
