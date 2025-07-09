using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Api.Configuration;

internal class HubspotFormsOperationIdHandler : OperationIdHandler
{
    public HubspotFormsOperationIdHandler(IOptions<ApiVersioningOptions> apiVersioningOptions) : base(apiVersioningOptions)
    {
    }

    protected override bool CanHandle(ApiDescription apiDescription, ControllerActionDescriptor controllerActionDescriptor)
        => controllerActionDescriptor.ControllerTypeInfo.Namespace?.StartsWith("Umbraco.Forms.Integrations.Crm.Hubspot") is true;
}
