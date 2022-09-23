using System.Collections.Generic;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Configuration
{
    public class PaymentProviderSettings
    {
        public string GatewayBaseUrl { get; set; }

        public string WpfUrl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Usage { get; set; }

        public string Supplier { get; set; }

        public string UmbracoBaseUrl { get; set; }

        public Dictionary<string, string> Currencies { get; set; }

        public string[] TransactionTypes { get; set; }

        public string[] MappingFields { get; set; }
    }
}