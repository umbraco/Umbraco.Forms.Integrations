using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Configuration;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Helpers;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos;

namespace Umbraco.Forms.Integrations.Commerce.emerchantpay.Controllers
{
    [PluginController("UmbracoFormsIntegrationsCommerceEmerchantpay")]
    public class EmerchantpayController : UmbracoAuthorizedApiController
    {
        private readonly PaymentProviderSettings _paymentProviderSettings;

        private readonly CurrencyHelper _currencyHelper;

        private readonly MappingFieldHelper _mappingFieldHelper;

        public EmerchantpayController(IOptions<PaymentProviderSettings> options, CurrencyHelper currencyHelper, MappingFieldHelper mappingFieldHelper)
        {
            _paymentProviderSettings = options.Value;

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