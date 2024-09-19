using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Forms.Core.Services;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Api.Management.Controllers.Forms
{
    [BackOfficeRoute($"{Constants.ManagementApi.RootPath}/v{{version:apiVersion}}/forms")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.AccountsGroupName)]
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    public class FormControllerBase : ActiveCampaignControllerBase
    {
        protected readonly IFormService FormService;

        public FormControllerBase(IFormService formService)
        {
            FormService = formService;
        }
    }
}
