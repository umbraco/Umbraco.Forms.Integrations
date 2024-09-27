using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Api.Management.Controllers.Accounts
{
    [ApiVersion("1.0")]
    [BackOfficeRoute($"{Constants.ManagementApi.RootPath}/v{{version:apiVersion}}/accounts")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.AccountsGroupName)]
    public class AccountControllerBase : ActiveCampaignControllerBase
    {
        protected readonly IAccountService _accountService;

        public AccountControllerBase(IAccountService accountService)
        {
            _accountService = accountService;
        }
    }
}
