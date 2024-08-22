using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Forms.Integrations.Crm.Hubspot.Filters;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Api.Management.Controllers.ShopifyCompliance
{
    [ApiVersion("1.0")]
    [BackOfficeRoute($"{Constants.ManagementApi.RootPath}/v{{version:apiVersion}}/shopify-compliance")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.ShopifyComplianceGroupName)]
    public class ShopifyComplianceControllerBase : HubspotControllerBase
    {
        public ShopifyComplianceControllerBase() { }

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
