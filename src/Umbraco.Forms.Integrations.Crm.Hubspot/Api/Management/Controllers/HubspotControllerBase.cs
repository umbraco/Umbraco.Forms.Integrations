﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Api.Management.Controllers
{
    [ApiController]
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    [MapToApi(Constants.ManagementApi.ApiName)]
    public class HubspotControllerBase : ControllerBase
    {
        
    }
}
