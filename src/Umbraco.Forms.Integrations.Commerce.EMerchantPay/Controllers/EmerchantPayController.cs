#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
#else
using System.Configuration;
using System.Web.Http;

using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
#endif

using System.Collections.Generic;

using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Helpers;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Configuration;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Controllers
{
    [PluginController("UmbracoFormsIntegrationsCommerceEmerchantpay")]
    public class EmerchantpayController : UmbracoAuthorizedApiController
    {
        private readonly PaymentProviderSettings _paymentProviderSettings;

        private readonly CurrencyHelper _currencyHelper;

        private readonly MappingFieldHelper _mappingFieldHelper;

#if NETCOREAPP
        public EmerchantpayController(IOptions<PaymentProviderSettings> options, CurrencyHelper currencyHelper, MappingFieldHelper mappingFieldHelper)
#else
        public EmerchantpayController(CurrencyHelper currencyHelper, MappingFieldHelper mappingFieldHelper)
#endif
        {
#if NETCOREAPP
            _paymentProviderSettings = options.Value;
#else
            _paymentProviderSettings = new PaymentProviderSettings(ConfigurationManager.AppSettings);
#endif

            _currencyHelper = currencyHelper;
            _mappingFieldHelper = mappingFieldHelper;
        }
        [HttpGet]
        public string IsAccountAvailable() =>
            string.IsNullOrEmpty(_paymentProviderSettings.Username) || string.IsNullOrEmpty(_paymentProviderSettings.Password)
            ? "UNAVAILABLE" : "AVAILABLE";

        [HttpGet]
        public IEnumerable<CurrencyDto> GetCurrencies() => _currencyHelper.GetCurrencies();

        [HttpGet]
        public IEnumerable<string> GetAvailableMappingFields() => new[]
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
