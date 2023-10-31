using System.Collections.Specialized;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Configuration
{
    public class ZapierCmsSettings : AppSettings
    {
        public ZapierCmsSettings()
        {
            
        }

        public ZapierCmsSettings(NameValueCollection appSettings)
        {
            UserGroupAlias = appSettings[Constants.UmbracoFormsIntegrationsAutomationZapierUserGroupAlias];

            ApiKey = appSettings[Constants.UmbracoFormsIntegrationsAutomationZapierApiKey];
        }
    }
}
