namespace Umbraco.Forms.Integrations.Crm.Hubspot.Services
{
    public enum AuthenticationMode
    {
        Unauthenticated,
        ApiKey,
        OAuth
    }

    public class AuthenticationDetail
    {
        public string ApiKey { get; set; }

        public string RefreshToken { get; set; }

        public AuthenticationMode Mode =>
            !string.IsNullOrEmpty(ApiKey)
                ? AuthenticationMode.ApiKey
                : !string.IsNullOrEmpty(RefreshToken)
                    ? AuthenticationMode.OAuth
                    : AuthenticationMode.Unauthenticated;
    }
}
