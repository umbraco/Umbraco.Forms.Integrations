namespace Umbraco.Forms.Integrations.Crm.Hubspot.Services
{
    public enum AuthenticationMode
    {
        Unauthenticated,
        ApiKey,
        PrivateAccessToken,
        OAuth
    }

    public class AuthenticationDetail
    {
        public string ApiKey { get; set; }

        public string PrivateAccessToken { get; set; }

        public string RefreshToken { get; set; }

        public AuthenticationMode Mode
        {
            get
            {
                if (string.IsNullOrEmpty(ApiKey)
                   && string.IsNullOrEmpty(PrivateAccessToken)
                   && string.IsNullOrEmpty(RefreshToken))
                    return AuthenticationMode.Unauthenticated;

                if (!string.IsNullOrEmpty(ApiKey))
                {
                    return AuthenticationMode.ApiKey;
                }
                else if (!string.IsNullOrEmpty(PrivateAccessToken))
                {
                    return AuthenticationMode.PrivateAccessToken;
                }

                return AuthenticationMode.OAuth;
            }
        }
    }
}
