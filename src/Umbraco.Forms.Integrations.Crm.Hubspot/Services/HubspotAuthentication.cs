namespace Umbraco.Forms.Integrations.Crm.Hubspot.Services
{
    public enum HubspotAuthenticationMode
    {
        Undefined,
        ApiKey,
        OAuth
    }

    public class HubspotAuthentication
    {
        public HubspotAuthentication(HubspotAuthenticationMode mode)
        {
            Mode = mode;
        }

        public HubspotAuthenticationMode Mode { get; }

        public string ApiKey { get; set; }

        public string OAuthAuthenticationCode { get; set; }
    }
}
