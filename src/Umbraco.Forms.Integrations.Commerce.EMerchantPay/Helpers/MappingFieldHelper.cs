#if NETCOREAPP
using Microsoft.Extensions.Options;
#else
using System.Configuration;
#endif

using System.Collections.Generic;
using System.Linq;

using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Configuration;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Helpers
{
    public class MappingFieldHelper
    {
        private readonly PaymentProviderSettings _paymentProviderSettings;

#if NETCOREAPP
        public MappingFieldHelper(IOptions<PaymentProviderSettings> options)
        {
            _paymentProviderSettings = options.Value;
        }
#else
        public MappingFieldHelper()
        {
            _paymentProviderSettings = new PaymentProviderSettings(ConfigurationManager.AppSettings);
        }
#endif

        public IEnumerable<string> GetMappings()
        {
            if (string.IsNullOrEmpty(_paymentProviderSettings.MappingFields))
                return Enumerable.Empty<string>();

            try
            {
                return _paymentProviderSettings.MappingFields.Split(';');
            }
            catch
            {
                return Enumerable.Empty<string>();
            }

        }

    }
}

