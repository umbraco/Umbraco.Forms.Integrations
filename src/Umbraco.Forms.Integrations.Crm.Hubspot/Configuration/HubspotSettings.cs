
using System.Collections.Specialized;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Configuration
{
    public class HubspotSettings
    {
        public HubspotSettings(NameValueCollection appSettings)
        {
            var settings = appSettings["Umbraco.Forms.Integrations.Crm.Hubspot.AllowContactUpdate"];
            AllowContactUpdate = bool.TryParse(settings, out var result)
                ? result : false;
        }

        public bool AllowContactUpdate { get; set; }
    }
}
