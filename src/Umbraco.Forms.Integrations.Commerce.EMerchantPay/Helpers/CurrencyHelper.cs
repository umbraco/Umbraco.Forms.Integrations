using System.Collections.Generic;

using System.Linq;

#if NETCOREAPP
using Microsoft.Extensions.Options;
#else
using System.Configuration;
#endif

using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Configuration;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Models.Dtos;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Helpers
{
    public class CurrencyHelper
    {
#if NETCOREAPP
        private readonly PaymentProviderSettings _paymentProviderSettings;

        public CurrencyHelper(IOptions<PaymentProviderSettings> options)
        {
            _paymentProviderSettings = options.Value;
        }
#endif

        public IEnumerable<CurrencyDto> GetCurrencies()
        {
#if NETCOREAPP
            var currencySettings = _paymentProviderSettings.Currencies;
#else
            var currencySettings = ConfigurationManager.AppSettings[Constants.Configuration.CurrenciesKey];
#endif

            if (string.IsNullOrEmpty(currencySettings)) return Enumerable.Empty<CurrencyDto>();

            try
            {
                var currenciesArr = currencySettings.Split(';');

                return currenciesArr.Select(p =>
                {
                    var currencyObj = p.Split(',');
                    return new CurrencyDto { Code = currencyObj[0], Name = currencyObj[1] };
                });
            }
            catch 
            {
                return Enumerable.Empty<CurrencyDto>();
            }

        }

    }
}
