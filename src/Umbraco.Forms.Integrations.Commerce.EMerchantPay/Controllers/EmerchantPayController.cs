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

        private readonly MappingFieldHelper _mappingFieldHelper;

        public EmerchantPayController(CurrencyHelper currencyHelper, MappingFieldHelper mappingFieldHelper)
        {
            _currencyHelper = currencyHelper;
            _mappingFieldHelper = mappingFieldHelper;
        }

        [HttpGet]
        public IEnumerable<CurrencyDto> GetCurrencies() => _currencyHelper.GetCurrencies();

        [HttpGet]
        public IEnumerable<string> GetDefaultMappingFields() => new[]
        {
            "Email",
            "FirstName",
            "LastName",
            "Address1",
            "Address2",
            "ZipCode",
            "City",
            "State",
            "Country",
            "Phone"
        };

        [HttpGet]
        public IEnumerable<string> GetRequiredMappingFields() => _mappingFieldHelper.GetMappings();
    }
}
