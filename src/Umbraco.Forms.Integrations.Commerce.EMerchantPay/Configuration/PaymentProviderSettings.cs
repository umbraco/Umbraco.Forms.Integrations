using System.Collections.Generic;
using System.Collections.Specialized;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Configuration
{
    public class PaymentProviderSettings
    {
        public PaymentProviderSettings()
        {
        }

        public PaymentProviderSettings(NameValueCollection appSettings)
        {
            GatewayBaseUrl = appSettings[Constants.Configuration.GatewayBaseUrlKey];

            WpfUrl = appSettings[Constants.Configuration.WpfUrlKey];

            Username = appSettings[Constants.Configuration.UsernameKey];

            Password = appSettings[Constants.Configuration.PasswordKey];

            Supplier = appSettings[Constants.Configuration.SupplierKey];

            Usage = appSettings[Constants.Configuration.UsageKey];

#if NETFRAMEWORK
            Currencies = appSettings[Constants.Configuration.CurrenciesKey];

            TransactionTypes = appSettings[Constants.Configuration.TransactionTypesKey];

            MappingFields = appSettings[Constants.Configuration.MappingFieldsKey];
#endif

            UmbracoBaseUrl = appSettings[Constants.Configuration.UmbracoBaseUrlKey];
        }

        public string GatewayBaseUrl { get; set; }

        public string WpfUrl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Usage { get; set; }

        public string Supplier { get; set; }

        public string UmbracoBaseUrl { get; set; }

#if NETCOREAPP
        public Dictionary<string, string> Currencies { get; set; }

        public string[] TransactionTypes { get; set; }

        public string[] MappingFields { get; set; }
#else
        public string Currencies { get; set; }

         public string TransactionTypes { get; set; }

         public string MappingFields { get; set; }
#endif


    }
}
