using System.Linq;

using Microsoft.Extensions.Options;

using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Controllers
{
    public class ZapierFormAuthorizedApiController : UmbracoApiController
    {
        private readonly ZapierSettings Options;

        private readonly IUserValidationService _userValidationService;

        public ZapierFormAuthorizedApiController(IOptions<ZapierSettings> options, IUserValidationService userValidationService)
        {
            Options = options.Value;

            _userValidationService = userValidationService;
        }

        public bool IsAccessValid()
        {
            string username = string.Empty;
            string password = string.Empty;
            string apiKey = string.Empty;

            if (Request.Headers.TryGetValue(Constants.ZapierAppConfiguration.UsernameHeaderKey,
                    out var usernameValues))
                username = usernameValues.First();
            if (Request.Headers.TryGetValue(Constants.ZapierAppConfiguration.PasswordHeaderKey,
                    out var passwordValues))
                password = passwordValues.First();
            if (Request.Headers.TryGetValue(Constants.ZapierAppConfiguration.ApiKeyHeaderKey,
                    out var apiKeyValues))
                apiKey = apiKeyValues.First();

            if (string.IsNullOrEmpty(apiKey) && (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))) return false;

            if (!string.IsNullOrEmpty(apiKey))
                return apiKey == Options.ApiKey;

            var isAuthorized = _userValidationService.Validate(username, password, Options.ApiKey).GetAwaiter()
                .GetResult();
            if (!isAuthorized) return false;

            return true;
        }
    }
}
