#if NETCOREAPP
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Configuration
{
    public class ObjectParser : ISettingsParser
    {
        private readonly PaymentProviderSettings _settings;

        public ObjectParser(IOptions<PaymentProviderSettings> options)
        {
            _settings = options.Value;
        }

        public Dictionary<string, string> AsDictionary(string propertyName)
        {
            var property = _settings.GetType().GetProperty(propertyName);

            if (property == null) throw new ArgumentNullException(nameof(ObjectParser));

            var propertyValues = property.GetValue(_settings) as Dictionary<string, string>;

            return propertyValues;
        }

        public IEnumerable<string> AsEnumerable(string propertyName)
        {
            var property = _settings.GetType().GetProperty(propertyName);

            if (property == null) throw new ArgumentNullException(nameof(ObjectParser));

            var propertyValues = property.GetValue(_settings);

            return propertyValues != null ? propertyValues as IEnumerable<string> : Enumerable.Empty<string>();
        }
    }
}
#endif
