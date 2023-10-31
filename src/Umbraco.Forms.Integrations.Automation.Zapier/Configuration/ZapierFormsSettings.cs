using System.Collections.Specialized;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Configuration
{
    public class ZapierFormsSettings : AppSettings
    {
        public ZapierFormsSettings()
        {

        }

        public ZapierFormsSettings(NameValueCollection appSettings)
        {
            UserGroupAlias = appSettings[Constants.UmbracoFormsIntegrationsAutomationZapierUserGroupAlias];

            ApiKey = appSettings[Constants.UmbracoFormsIntegrationsAutomationZapierApiKey];
        }
    }
}
