using System.Collections.Specialized;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Configuration
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
        }

        public string GatewayBaseUrl { get; set; }

        public string WpfUrl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
