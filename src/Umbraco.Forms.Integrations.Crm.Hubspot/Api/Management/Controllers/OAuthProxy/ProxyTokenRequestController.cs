using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Forms.Integrations.Crm.Hubspot.Configuration;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Api.Management.Controllers.OAuthProxy
{
    public class ProxyTokenRequestController : OAuthProxyControllerBase
    {
        public ProxyTokenRequestController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings) : base(httpClientFactory, appSettings)
        {
        }

        [HttpPost("token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Token()
        {
            return Ok(ProxyTokenRequest());
        }
    }
}
