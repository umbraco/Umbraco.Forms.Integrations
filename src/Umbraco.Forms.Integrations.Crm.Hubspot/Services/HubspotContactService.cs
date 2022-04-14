using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Integrations.Crm.Hubspot.Extensions;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models.Responses;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Services
{
    public class HubspotContactService : IContactService
    {
        // Using a static HttpClient (see: https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/).
        private readonly static HttpClient s_client = new HttpClient();

        // Access to the client within the class is via ClientFactory(), allowing us to mock the responses in tests.
        internal static Func<HttpClient> ClientFactory = () => s_client;

        private readonly IFacadeConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly AppCaches _appCaches;
        private readonly IKeyValueService _keyValueService;

        private const string CrmApiBaseUrl = "https://api.hubapi.com/crm/v3/";
        private const string InstallUrlFormat = "https://app-eu1.hubspot.com/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}";
        private const string OAuthScopes = "oauth%20forms%20crm.objects.contacts.read%20crm.objects.contacts.write";
        private const string OAauthClientId = "1a04f5bf-e99e-48e1-9d62-6c25bf2bdefe";

        private const string OAuthBaseUrl = "https://hubspot-forms-auth.umbraco.com/";  // For local testing: "https://localhost:44364/"
        private static string OAuthRedirectUrl = OAuthBaseUrl;
        private static string OAuthTokenProxyUrl = $"{OAuthBaseUrl}oauth/v1/token";

        private const string AccessTokenCacheKey = "HubSpotOAuthAccessToken";

        private const string RefreshTokenDatabaseKey = "Umbraco.Forms.Integrations.Crm.Hubspot+RefreshToken";

        public HubspotContactService(IFacadeConfiguration configuration, ILogger logger, AppCaches appCaches, IKeyValueService keyValueService)
        {
            _configuration = configuration;
            _logger = logger;
            _appCaches = appCaches;
            _keyValueService = keyValueService;
        }

        public AuthenticationMode IsAuthorizationConfigured() => GetConfiguredAuthenticationDetails().Mode;

        public string GetAuthenticationUrl() => string.Format(InstallUrlFormat, OAauthClientId, OAuthRedirectUrl, OAuthScopes);

        public async Task<AuthorizationResult> AuthorizeAsync(string code)
        {
            var tokenRequest = new GetTokenRequest
            {
                ClientId = OAauthClientId,
                RedirectUrl = OAuthRedirectUrl,
                AuthorizationCode = code,
            };
            var response = await GetResponse(OAuthTokenProxyUrl, HttpMethod.Post, content: tokenRequest, contentType: "application/x-www-form-urlencoded").ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadAsAsync<TokenResponse>();

                // Add the access token details to the cache.
                _appCaches.RuntimeCache.InsertCacheItem(AccessTokenCacheKey, () => tokenResponse.AccessToken);

                // Save the refresh token into the database.
                _keyValueService.SetValue(RefreshTokenDatabaseKey, tokenResponse.RefreshToken);

                return AuthorizationResult.AsSuccess();
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.Error<HubspotContactService>($"Could not retrieve authenticate with HubSpot API. Status code: {response.StatusCode}, reason: {response.ReasonPhrase}, content: {responseContent}");
                return AuthorizationResult.AsError("Could not retrieve OAuth token from HubSpot API, see log for details.");
            }
        }

        public AuthorizationResult Deauthorize()
        {
            // Delete any saved refresh token.
            _keyValueService.SetValue(RefreshTokenDatabaseKey, string.Empty);
            return AuthorizationResult.AsSuccess();
        }

        public async Task<IEnumerable<Property>> GetContactPropertiesAsync()
        {
            var authenticationDetails = GetConfiguredAuthenticationDetails();
            if (authenticationDetails.Mode == AuthenticationMode.Unauthenticated)
            {
                _logger.Info<HubspotContactService>("Cannot access HubSpot API via API key or OAuth, as neither a key has been configured nor a refresh token stored.");
                return Enumerable.Empty<Property>();
            }

            var requestUrl = $"{CrmApiBaseUrl}properties/contacts";
            var httpMethod = HttpMethod.Get;
            var response = await GetResponse(requestUrl, httpMethod, authenticationDetails).ConfigureAwait(false);
            if (response.IsSuccessStatusCode == false)
            {
                var retryResult = await HandleFailedRequest(response.StatusCode, requestUrl, httpMethod, authenticationDetails);
                if (retryResult.Success)
                {
                    response = retryResult.RetriedResponse;
                }
                else
                {
                    _logger.Error<HubspotContactService>("Failed to fetch contact properties from HubSpot API for mapping. {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                    return Enumerable.Empty<Property>();
                }
            }

            // Map the properties to our simpler object, as we don't need all the fields in the response.
            var properties = new List<Property>();
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseContentAsJson = JsonConvert.DeserializeObject<PropertiesResponse>(responseContent);
            properties.AddRange(responseContentAsJson.Results);
            return properties.OrderBy(x => x.Label);
        }

        public async Task<CommandResult> PostContactAsync(Record record, List<MappedProperty> fieldMappings)
        {
            var authenticationDetails = GetConfiguredAuthenticationDetails();
            if (authenticationDetails.Mode == AuthenticationMode.Unauthenticated)
            {
                _logger.Warn<HubspotContactService>("Cannot access HubSpot API via API key or OAuth, as neither a key has been configured nor a refresh token stored.");
                return CommandResult.NotConfigured;
            }

            // Map data from the workflow setting Hubspot fields
            // From the form field values submitted for this form submission
            var postData = new PropertiesRequest();
            foreach (var mapping in fieldMappings)
            {
                var fieldId = mapping.FormField;
                var recordField = record.GetRecordField(Guid.Parse(fieldId));
                if (recordField != null)
                {
                    // TODO: What about different field types in forms & Hubspot that are not simple text ones?
                    postData.Properties.Add(mapping.HubspotField, recordField.ValuesAsString(false));
                }
                else
                {
                    // The field mapping value could not be found so write a warning in the log.
                    _logger.Warn<HubspotContactService>("The field mapping with Id, {FieldMappingId}, did not match any record fields. This is probably caused by the record field being marked as sensitive and the workflow has been set not to include sensitive data", mapping.FormField);
                }
            }

            // POST data to hubspot
            // https://api.hubapi.com/crm/v3/objects/contacts?hapikey=YOUR_HUBSPOT_API_KEY
            var requestUrl = $"{CrmApiBaseUrl}objects/contacts";
            var httpMethod = HttpMethod.Post;
            var response = await GetResponse(requestUrl, httpMethod, authenticationDetails, postData, "application/json").ConfigureAwait(false);

            // Depending on POST status fail or mark workflow as completed
            if (response.IsSuccessStatusCode == false)
            {
                var retryResult = await HandleFailedRequest(response.StatusCode, requestUrl, httpMethod, authenticationDetails);
                if (retryResult.Success)
                {
                    return CommandResult.Success;
                }
                else
                {
                    _logger.Error<HubspotContactService>("Error submitting a HubSpot contact request ");
                    return CommandResult.Failed;
                }
            }

            return CommandResult.Success;
        }

        private AuthenticationDetail GetConfiguredAuthenticationDetails()
        {
            var authentication = new AuthenticationDetail();
            if (TryGetApiKey(out string apiKey))
            {
                authentication.ApiKey = apiKey;
            }
            else if (TryGetSavedRefreshToken(out string refreshToken))
            {
                authentication.RefreshToken = refreshToken;
            }

            return authentication;
        }

        private bool TryGetApiKey(out string apiKey)
        {
            apiKey = _configuration.GetSetting("HubSpotApiKey");
            return !string.IsNullOrEmpty(apiKey);
        }

        private bool TryGetSavedRefreshToken(out string refreshToken)
        {
            refreshToken = _keyValueService.GetValue(RefreshTokenDatabaseKey);
            return !string.IsNullOrEmpty(refreshToken);
        }

        private async Task<string> GetOAuthAccessTokenFromCacheOrRefreshToken(string refreshToken)
        {
            var accessToken = _appCaches.RuntimeCache.GetCacheItem<string>(AccessTokenCacheKey);
            if (string.IsNullOrEmpty(accessToken))
            {
                // No access token in the cache, so get a new one from the refresh token.
                await RefreshOAuthAccessToken(refreshToken);
                accessToken = _appCaches.RuntimeCache.GetCacheItem<string>(AccessTokenCacheKey);
            }

            return accessToken;
        }

        private async Task RefreshOAuthAccessToken(string refreshToken)
        {
            var tokenRequest = new RefreshTokenRequest
            {
                ClientId = OAauthClientId,
                RedirectUrl = OAuthRedirectUrl,
                RefreshToken = refreshToken,
            };
            var response = await GetResponse(OAuthTokenProxyUrl, HttpMethod.Post, content: tokenRequest, contentType: "application/x-www-form-urlencoded").ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadAsAsync<TokenResponse>();

                // Update the token details in the cache.
                _appCaches.RuntimeCache.InsertCacheItem(AccessTokenCacheKey, () => tokenResponse.AccessToken);

                // Update the saved refresh token if we've got a different one.
                if (tokenResponse.RefreshToken != refreshToken)
                {
                    _keyValueService.SetValue(RefreshTokenDatabaseKey, tokenResponse.RefreshToken);
                }
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                // Failed to refresh an access token from a refresh token, so remove what we have stored.
                // Re-authentication via the authorization code will be required.
                _keyValueService.SetValue(RefreshTokenDatabaseKey, string.Empty);

                throw new InvalidOperationException(
                    $"Could not refresh OAuth token from HubSpot API. Status code: {response.StatusCode}, reason: {response.ReasonPhrase}, content: {responseContent}");
            }
        }

        private async Task<HttpResponseMessage> GetResponse(
            string url,
            HttpMethod httpMethod,
            AuthenticationDetail authenticationDetails = null,
            object content = null,
            string contentType = null)
        {
            var requestMessage = new HttpRequestMessage
            {
                Method = httpMethod,
                RequestUri = new Uri(url),
                Content = CreateRequestContent(content, contentType),
            };

            if (authenticationDetails != null)
            {
                // Apply appropriate authentication details to the request.  Either the API key as a querystring or the access token as a header.
                switch (authenticationDetails.Mode)
                {
                    case AuthenticationMode.ApiKey:
                        requestMessage.RequestUri = new Uri($"{url}?hapikey={authenticationDetails.ApiKey}");
                        break;
                    case AuthenticationMode.OAuth:
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", await GetOAuthAccessTokenFromCacheOrRefreshToken(authenticationDetails.RefreshToken));
                        break;
                }
            }

            return await ClientFactory().SendAsync(requestMessage).ConfigureAwait(false);
        }

        private static HttpContent CreateRequestContent(object data, string contentType)
        {
            if (data == null)
            {
                return null;
            }

            switch (contentType)
            {
                case "application/json":
                    var serializedData = JsonConvert.SerializeObject(data);
                    return new StringContent(serializedData, Encoding.UTF8, contentType);
                case "application/x-www-form-urlencoded":
                    return new FormUrlEncodedContent(data.AsDictionary());
                default:
                    throw new InvalidOperationException($"Unexpected content type: {contentType}");
            }
        }

        private async Task<HandleFailedRequestResult> HandleFailedRequest(HttpStatusCode statusCode, string requestUrl, HttpMethod httpMethod, AuthenticationDetail authenticationDetails)
        {
            var result = new HandleFailedRequestResult();
            if (authenticationDetails.Mode == AuthenticationMode.OAuth)
            {
                if (statusCode == HttpStatusCode.Unauthorized)
                {
                    // If we've got a 401 response and are using OAuth, likely our access token has expired.
                    // First we should try to refresh it using the refresh token.  If successful this will save the new
                    // value into the cache.
                    await RefreshOAuthAccessToken(authenticationDetails.RefreshToken);

                    // Repeat the operation using the refreshed token.
                    var response = await GetResponse(requestUrl, httpMethod, authenticationDetails).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        result.Success = true;
                        result.RetriedResponse = response;
                    }
                }
                else if (statusCode == HttpStatusCode.Forbidden)
                {
                    HandleForbiddenResponse();
                }
            }

            return result;
        }

        private void HandleForbiddenResponse()
        {
            // Token is no longer valid (perhaps due to additional scopes requested).  Need to clear and re-authenticate.
            _keyValueService.SetValue(RefreshTokenDatabaseKey, string.Empty);
        }

        private class HandleFailedRequestResult
        {
            public bool Success { get; set; }

            public HttpResponseMessage RetriedResponse { get; set; }
        }
    }
}
