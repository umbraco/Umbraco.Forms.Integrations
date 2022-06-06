using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Models.Dtos;


namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Controllers
{
    [PluginController("UmbracoFormsIntegrationsCommerceEmerchantPay")]
    public class EmerchantPayController : UmbracoAuthorizedApiController
    {
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

#if NETCOREAPP
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmerchantPayController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
#endif

        [HttpGet]
        public IEnumerable<CurrencyDto> GetCurrencies()
        {
#if NETCOREAPP
            string currenciesFilePath =
                $"{_webHostEnvironment.ContentRootPath}/App_Plugins/UmbracoForms.Integrations/Commerce/eMerchantPay/currencies.json";
#else
            string currenciesFilePath =
                HttpContext.Current.Server.MapPath(
                    "~/App_Plugins/UmbracoForms.Integrations/Commerce/eMerchantPay/currencies.json");
#endif

            _lock.EnterReadLock();

            try
            {
                var content = System.IO.File.ReadAllText(currenciesFilePath);

                var d = JsonConvert.DeserializeObject<IEnumerable<CurrencyDto>>(content);

                return d;
            }
            catch (FileNotFoundException ex)
            {
                return Enumerable.Empty<CurrencyDto>();
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<CurrencyDto>();
            }
        }
    }
}
