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
            UserGroup = appSettings[Constants.UmbracoFormsIntegrationsAutomationZapierUserGroup];
        }

        public string UserGroup { get; set; }
    }
}
