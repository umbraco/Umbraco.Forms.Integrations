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

using Umbraco.Web.WebApi;
#endif

namespace Umbraco.Forms.Integrations.Automation.Zapier.Controllers
{
    /// <summary>
    /// When a Zapier user creates a "New Form Submitted" trigger, he is authenticated, then selects a form, the API provides an output json with the
    /// structure of the selected form.
    /// </summary>
    public class FormPollingController : UmbracoApiController
    {
        private readonly ZapierSettings Options;

        private readonly ZapierFormService _zapierFormService;

        private readonly IUserValidationService _userValidationService;

#if NETCOREAPP
        public FormPollingController(IOptions<ZapierSettings> options, ZapierFormService zapierFormService, IUserValidationService userValidationService)
#else
        public FormPollingController(ZapierFormService zapierFormService, IUserValidationService userValidationService)
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

        public IEnumerable<FormDto> GetSampleForm()
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

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return null;

            var isAuthorized = _userValidationService.Validate(username, password, Options.UserGroup).GetAwaiter()
                .GetResult();
            if (!isAuthorized) return null;

            return _zapierFormService.GetAll().Take(1);
        }
    }
}
