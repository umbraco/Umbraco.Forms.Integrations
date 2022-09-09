using System;
using System.Collections.Generic;
using System.Linq;

using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Configuration;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Helpers
{
    public class MappingFieldHelper
    {
        private readonly ISettingsParser _settingsParser;

        public MappingFieldHelper(ISettingsParser settingsParser)
        {
            _settingsParser = settingsParser;
        }

        public IEnumerable<string> GetMappings()
        {
            var mappings = _settingsParser.AsEnumerable(nameof(PaymentProviderSettings.MappingFields));

            if (mappings.Count() == 0) throw new ArgumentNullException(nameof(MappingFieldHelper));

            return mappings;

        }

    }
}

