using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Api.Management.Controllers.Accounts
{
    [ApiVersion("1.0")]
    public class GetAccountController : AccountControllerBase
    {
        public GetAccountController(IAccountService accountService) : base(accountService)
        {
        }

        [HttpGet]
        [ProducesResponseType(typeof(AccountCollectionResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAccounts()
        {
            var accounts = await _accountService.Get();

            return Ok(accounts);
        }
    }
}
