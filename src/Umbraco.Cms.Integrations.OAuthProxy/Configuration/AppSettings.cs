namespace Umbraco.Forms.Integrations.Crm.Hubspot.OAuthProxy.Configuration
{
    public class AppSettings
    {
        public string HubspotAuthUri { get; set; }

        public string HubspotClientSecret { get; set; }

        public string SemrushAuthUri { get; set; }

        public string SemrushClientSecret { get; set; }

        public string this[string propertyName] => (string) GetType().GetProperty(propertyName)?.GetValue(this, null);
    }
}
