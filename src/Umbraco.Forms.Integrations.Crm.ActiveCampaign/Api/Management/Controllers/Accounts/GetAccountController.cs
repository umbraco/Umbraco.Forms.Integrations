using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Api.Management.Controllers.Accounts
{
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

            return Ok(new JsonResult(accounts));
        }
    }
}
