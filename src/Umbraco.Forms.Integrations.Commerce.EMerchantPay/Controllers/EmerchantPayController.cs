using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

#if NETCOREAPP
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
#else
using System.Web;
using System.Web.Http;

using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
#endif
using Newtonsoft.Json;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Helpers;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Models.Dtos;


namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Controllers
{
    [PluginController("UmbracoFormsIntegrationsCommerceEmerchantPay")]
    public class EmerchantPayController : UmbracoAuthorizedApiController
    {
        private readonly CurrencyHelper _currencyHelper;

        public EmerchantPayController(CurrencyHelper currencyHelper)
        {
            _currencyHelper = currencyHelper;
        }

        [HttpGet]
        public IEnumerable<CurrencyDto> GetCurrencies() => _currencyHelper.GetCurrencies();
    }
}
