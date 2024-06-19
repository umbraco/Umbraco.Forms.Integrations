using Microsoft.AspNetCore.Mvc;

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
        public IActionResult CustomerDataRequest() => Ok();

        /// <summary>
        /// Handles customer data erasure requests from Shopify
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/shopify-compliance/v1/customer/data-redact")]
        public IActionResult CustomerDataRedact() => Ok();

        /// <summary>
        /// Handles shop data erasure requests from Shopify
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/shopify-compliance/v1/shop/data-redact")]
        public IActionResult ShopDataRedact() => Ok();
    }
}
