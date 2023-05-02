using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Umbraco.Forms.Integrations.Testsite.V10;

public class HubspotPrivateAccessTokenFilterAttribute : IActionFilter
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<HubspotPrivateAccessTokenFilterAttribute> _logger;

    public HubspotPrivateAccessTokenFilterAttribute(
        IConfiguration configuration,
        ILogger<HubspotPrivateAccessTokenFilterAttribute> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var privateAccessToken = _configuration.GetSection(Constants.HubspotSettingsPath)["PrivateAccessToken"];
        if(string.IsNullOrEmpty(privateAccessToken))
        {
            const string message = "Cannot access HubSpot API. Private Access Token is either missing or invalid.";
            _logger.LogInformation(message);
            context.Result = new UnauthorizedObjectResult(message);
        }
    }
}
