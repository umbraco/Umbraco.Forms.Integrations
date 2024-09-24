using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Forms.Core.Services;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Api.Management.Controllers.Forms
{
    [ApiVersion("1.0")]
    [BackOfficeRoute($"{Constants.ManagementApi.RootPath}/v{{version:apiVersion}}/forms")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.FormsGroupName)]
    public class FormControllerBase : ActiveCampaignControllerBase
    {
        protected readonly IFormService FormService;

        public FormControllerBase(IFormService formService)
        {
            FormService = formService;
        }
    }
}
