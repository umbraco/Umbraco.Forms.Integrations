using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Forms.Integrations.Crm.Hubspot.Services;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Filters;

public class ConfiguredAuthenticationFilterAttribute : IActionFilter
{
    private readonly ILogger<ConfiguredAuthenticationFilterAttribute> _logger;
    private readonly IAuthenticationService _authenticationService;

    public ConfiguredAuthenticationFilterAttribute(
        ILogger<ConfiguredAuthenticationFilterAttribute> logger, 
        IAuthenticationService authenticationService)
    {
        _logger = logger;
        _authenticationService = authenticationService;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var authenticationDetails = _authenticationService.GetDetails();
        if (authenticationDetails.Mode == AuthenticationMode.Unauthenticated)
        {
            const string message = "Cannot access HubSpot API via API key or OAuth, as neither a key has been configured nor a refresh token stored.";
            _logger.LogInformation(message);
            context.Result = new UnauthorizedObjectResult(message);
        }
    }
}
