#if NETFRAMEWORK
using System;
using System.Configuration;
using System.Collections.Generic;

using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Configuration;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Services
{
    public class StringParser : ISettingsParser
    {
        private readonly PaymentProviderSettings _settings;

        public StringParser()
        {
            _settings = new PaymentProviderSettings(ConfigurationManager.AppSettings);
        }

        public IEnumerable<string> AsEnumerable(string propertyName)
        {
            var property = _settings.GetType().GetProperty(propertyName);

            if (property == null) throw new ArgumentNullException(nameof(StringParser));

            return property.GetValue(_settings).ToString().Split(';');
        }

        public Dictionary<string, string> AsDictionary(string propertyName)
        {
            var dict = new Dictionary<string, string>();

            var property = _settings.GetType().GetProperty(propertyName);

            if (property == null) throw new ArgumentNullException(nameof(StringParser));

            var propertyValues = property.GetValue(_settings).ToString().Split(';');

            foreach(var propertyValue in propertyValues)
            {
                var value = propertyValue.Split(',');

                if(value.Length > 1)
                    dict.Add(value[0], value[1]);
            }

            return dict;
        }

    }
}
#endif
