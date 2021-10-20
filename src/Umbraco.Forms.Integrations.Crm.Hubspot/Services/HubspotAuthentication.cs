namespace Umbraco.Forms.Integrations.Crm.Hubspot.Services
{
    public enum HubspotAuthenticationMode
    {
        Unauthenticated,
        ApiKey,
        OAuth
    }

    public class HubspotAuthentication
    {
        public string ApiKey { get; set; }

        public string RefreshToken { get; set; }

        public HubspotAuthenticationMode Mode =>
            !string.IsNullOrEmpty(ApiKey)
                ? HubspotAuthenticationMode.ApiKey
                : !string.IsNullOrEmpty(RefreshToken)
                    ? HubspotAuthenticationMode.OAuth
                    : HubspotAuthenticationMode.Unauthenticated;
    }
}
