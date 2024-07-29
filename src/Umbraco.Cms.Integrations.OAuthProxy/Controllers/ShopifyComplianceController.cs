using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Integrations.OAuthProxy.Filters;

namespace Umbraco.Cms.Integrations.OAuthProxy.Controllers
{
	[ApiController]
	public class ShopifyComplianceController : Controller
    {
        /// <summary>
        /// Handles customer data requests from Shopify
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/shopify-compliance/v1/customer/data-request")]
		[SignatureValidation]
		public IActionResult CustomerDataRequest() => Ok();

        /// <summary>
        /// Handles customer data erasure requests from Shopify
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/shopify-compliance/v1/customer/data-redact")]
		[SignatureValidation]
		public IActionResult CustomerDataRedact() => Ok();

        /// <summary>
        /// Handles shop data erasure requests from Shopify
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/shopify-compliance/v1/shop/data-redact")]
		[SignatureValidation]
		public IActionResult ShopDataRedact() => Ok();
    }
}
