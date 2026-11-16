namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Configuration
{
    /// <summary>
    /// Single definition of "ActiveCampaign is configured" shared by the HTTP client setup, the
    /// access check endpoint and the workflow.
    /// </summary>
    internal static class ApiConfiguration
    {
        /// <summary>
        /// Builds the API base address (<c>{BaseUrl}/api/3/</c>) when <paramref name="baseUrl"/> is an absolute http(s) URL.
        /// </summary>
        public static bool TryGetApiBaseAddress(string? baseUrl, out Uri? apiBaseAddress)
        {
            apiBaseAddress = null;

            if (string.IsNullOrWhiteSpace(baseUrl)
                || !Uri.TryCreate($"{baseUrl.Trim().TrimEnd('/')}/api/3/", UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return false;
            }

            apiBaseAddress = uri;
            return true;
        }

        public static bool IsValid(string? baseUrl, string? apiKey) =>
            !string.IsNullOrWhiteSpace(apiKey) && TryGetApiBaseAddress(baseUrl, out _);
    }
}
