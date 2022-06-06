using System.Collections.Specialized;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Configuration
{
    public class ZapierSettings
    {
        public ZapierSettings()
        {
            
        }

        public ZapierSettings(NameValueCollection appSettings)
        {
            UserGroupAlias = appSettings[Constants.UmbracoFormsIntegrationsAutomationZapierUserGroupAlias];

            ApiKey = appSettings[Constants.UmbracoFormsIntegrationsAutomationZapierApiKey];
        }

        public string UserGroupAlias { get; set; }

        public string ApiKey { get; set; }
    }
}
