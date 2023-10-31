using System.Linq;

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
    public class ZapierFormAuthorizedApiController : UmbracoApiController
    {
        private readonly ZapierCmsSettings Options;

        private readonly IUserValidationService _userValidationService;

#if NETCOREAPP
        public ZapierFormAuthorizedApiController(IOptions<ZapierCmsSettings> options, IUserValidationService userValidationService)
#else
        public ZapierFormAuthorizedApiController(IUserValidationService userValidationService)
#endif
        {
#if NETCOREAPP
            Options = options.Value;
#else
            Options = new ZapierCmsSettings(ConfigurationManager.AppSettings);
#endif

            _userValidationService = userValidationService;
        }

        public bool IsAccessValid()
        {
            string username = string.Empty;
            string password = string.Empty;
            string apiKey = string.Empty;

#if NETCOREAPP
            if (Request.Headers.TryGetValue(Constants.ZapierAppConfiguration.UsernameHeaderKey,
                    out var usernameValues))
                username = usernameValues.First();
            if (Request.Headers.TryGetValue(Constants.ZapierAppConfiguration.PasswordHeaderKey,
                    out var passwordValues))
                password = passwordValues.First();
            if (Request.Headers.TryGetValue(Constants.ZapierAppConfiguration.ApiKeyHeaderKey,
                    out var apiKeyValues))
                apiKey = apiKeyValues.First();
#else
            if (Request.Headers.TryGetValues(Constants.ZapierAppConfiguration.UsernameHeaderKey,
                    out var usernameValues))
                username = usernameValues.First();
            if (Request.Headers.TryGetValues(Constants.ZapierAppConfiguration.PasswordHeaderKey,
                    out var passwordValues))
                password = passwordValues.First();
            if (Request.Headers.TryGetValues(Constants.ZapierAppConfiguration.ApiKeyHeaderKey,
                    out var apiKeyValues))
                apiKey = apiKeyValues.First();
#endif

            if (string.IsNullOrEmpty(apiKey) && (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))) return false;

            var isAuthorized = _userValidationService.Validate(username, password, Options.ApiKey).GetAwaiter()
                .GetResult();
            if (!isAuthorized) return false;

            return true;
        }
    }
}
