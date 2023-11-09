using System.Linq;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Controllers;

public class ZapierFormAuthorizedApiController : UmbracoApiController
{
    private readonly IUserValidationService _userValidationService;

    public ZapierFormAuthorizedApiController(
        IUserValidationService userValidationService) =>
        _userValidationService = userValidationService;

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

        var isAuthorized = _userValidationService.Validate(username, password, apiKey).GetAwaiter()
            .GetResult();
        if (!isAuthorized) return false;

        return true;
    }
}
