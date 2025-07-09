using Umbraco.Cms.Api.Management.OpenApi;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Api.Configuration;

public class BackOfficeSecurityRequirementsOperationFilter : BackOfficeSecurityRequirementsOperationFilterBase
{
    protected override string ApiName => Constants.ManagementApi.ApiName;
}
