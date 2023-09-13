using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Services;
using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Integrations.Crm.Hubspot.Configuration;
using Umbraco.Forms.Integrations.Crm.Hubspot.Extensions;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models.Responses;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Services
{
    public class HubspotContactService : IContactService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger<HubspotContactService> _logger;
        private readonly AppCaches _appCaches;
        private readonly IKeyValueService _keyValueService;

        private readonly HubspotSettings _settings;

        private const string CrmApiHost = "https://api.hubapi.com";
        private static readonly string CrmV3ApiBaseUrl = $"{CrmApiHost}/crm/v3/";
        private const string InstallUrlFormat = "https://app-eu1.hubspot.com/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}";
        private const string OAuthScopes = "oauth%20forms%20crm.objects.contacts.read%20crm.objects.contacts.write";
        private const string OAuthClientId = "1a04f5bf-e99e-48e1-9d62-6c25bf2bdefe";
        private const string JsonContentType = "application/json";

        private const string OAuthBaseUrl = "https://hubspot-forms-auth.umbraco.com/";  // For local testing: "https://localhost:44364/"
        private static string OAuthRedirectUrl = OAuthBaseUrl;
        private static string OAuthTokenProxyUrl = $"{OAuthBaseUrl}oauth/v1/token";

        private const string AccessTokenCacheKey = "HubSpotOAuthAccessToken";

        private const string RefreshTokenDatabaseKey = "Umbraco.Forms.Integrations.Crm.Hubspot+RefreshToken";

        public HubspotContactService(
            IHttpClientFactory httpClientFactory,
            IOptions<HubspotSettings> options, 
            ILogger<HubspotContactService> logger, 
            AppCaches appCaches, 
            IKeyValueService keyValueService)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
            _logger = logger;
            _appCaches = appCaches;
            _keyValueService = keyValueService;
        }

        public AuthenticationMode IsAuthorizationConfigured() => GetConfiguredAuthenticationDetails().Mode;

        public string GetAuthenticationUrl() => string.Format(InstallUrlFormat, OAuthClientId, OAuthRedirectUrl, OAuthScopes);

        public async Task<AuthorizationResult> AuthorizeAsync(string code)
        {
            var tokenRequest = new GetTokenRequest
            {
                ClientId = OAuthClientId,
                RedirectUrl = OAuthRedirectUrl,
                AuthorizationCode = code,
            };
            var response = await GetResponse(OAuthTokenProxyUrl, 
                HttpMethod.Post, content: tokenRequest, contentType: "application/x-www-form-urlencoded").ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                // Add the access token details to the cache.
                _appCaches.RuntimeCache.Insert(AccessTokenCacheKey, () => tokenResponse.AccessToken);

                // Save the refresh token into the database.
                _keyValueService.SetValue(RefreshTokenDatabaseKey, tokenResponse.RefreshToken);

                return AuthorizationResult.AsSuccess();
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Could not retrieve authenticate with HubSpot API. Status code: {response.StatusCode}, reason: {response.ReasonPhrase}, content: {responseContent}");
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
                _logger.LogInformation("Cannot access HubSpot API via API key or OAuth, as neither a key has been configured nor a refresh token stored.");
                return Enumerable.Empty<Property>();
            }

            var requestUrl = $"{CrmV3ApiBaseUrl}properties/contacts";
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
                    _logger.LogError("Failed to fetch contact properties from HubSpot API for mapping. {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
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
            => await PostContactAsync(record, fieldMappings, null);

        public async Task<CommandResult> PostContactAsync(Record record, List<MappedProperty> fieldMappings, Dictionary<string, string> additionalFields)
        {
            var authenticationDetails = GetConfiguredAuthenticationDetails();
            if (authenticationDetails.Mode == AuthenticationMode.Unauthenticated)
            {
                _logger.LogWarning("Cannot access HubSpot API via API key or OAuth, as neither a key has been configured nor a refresh token stored.");
                return CommandResult.NotConfigured;
            }

            // Map data from the workflow setting Hubspot fields
            // From the form field values submitted for this form submission
            var propertiesRequestV1 = new PropertiesRequestV1();
            var propertiesRequestV3 = new PropertiesRequestV3();
            var emailValue = string.Empty;
            foreach (var mapping in fieldMappings)
            {
                var fieldId = mapping.FormField;
                var recordField = record.GetRecordField(Guid.Parse(fieldId));
                if (recordField != null)
                {
                    var value = _settings.AllowContactUpdate && mapping.AppendValue
                        ? ";" + recordField.ValuesAsHubspotString(false)
                        : recordField.ValuesAsHubspotString(false);

                    propertiesRequestV1.Properties.Add(new PropertiesRequestV1.PropertyValue(mapping.HubspotField, value));
                    propertiesRequestV3.Properties.Add(mapping.HubspotField, value);

                    // TODO: What about different field types in forms & Hubspot that are not simple text ones?

                    // "Email" appears to be a special form field used for uniqueness checks, so we can safely look it up by name.
                    if (mapping.HubspotField.ToLowerInvariant() == "email")
                    {
                        emailValue = value;
                    }
                }
                else
                {
                    // The field mapping value could not be found so write a warning in the log.
                    _logger.LogWarning("The field mapping with Id, {FieldMappingId}, did not match any record fields. This is probably caused by the record field being marked as sensitive and the workflow has been set not to include sensitive data", mapping.FormField);
                }
            }

            if (additionalFields != null)
            {
                // Add any extra fields that got passed (from a custom workflow)
                foreach (var additionalField in additionalFields)
                {
                    propertiesRequestV1.Properties.Add(new PropertiesRequestV1.PropertyValue(additionalField.Key, additionalField.Value));
                    propertiesRequestV3.Properties.Add(additionalField.Key, additionalField.Value);
                }
            }

            // POST data to hubspot
            // https://api.hubapi.com/crm/v3/objects/contacts?hapikey=YOUR_HUBSPOT_API_KEY
            var requestUrl = $"{CrmV3ApiBaseUrl}objects/contacts";
            var httpMethod = HttpMethod.Post;
            var response = await GetResponse(requestUrl, httpMethod, authenticationDetails, propertiesRequestV3, JsonContentType).ConfigureAwait(false);

            // Depending on POST status fail or mark workflow as completed
            if (response.IsSuccessStatusCode == false)
            {
                // A 409 - Conflict response indicates that the contact (by email address) already exists.
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    if (!_settings.AllowContactUpdate)
                        _logger.LogInformation("Contact already exists in HubSpot CRM and workflow is configured to not apply updates, so update of information was skipped.");
                    return _settings.AllowContactUpdate
                        ? await UpdateContactAsync(record, authenticationDetails, propertiesRequestV1, emailValue)
                        : CommandResult.Success;
                }
                else
                {
                    var retryResult = await HandleFailedRequest(response.StatusCode, requestUrl, httpMethod, authenticationDetails, propertiesRequestV3, JsonContentType);
                    if (retryResult.Success)
                    {
                        _logger.LogInformation($"Hubspot contact record created from record {record.UniqueId}.");
                        return CommandResult.Success;
                    }
                    else
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Error creating a HubSpot contact: {responseContent}");
                        return CommandResult.Failed;
                    }
                }
            }
            else
            {
                _logger.LogInformation($"Hubspot contact record created from record {record.UniqueId}.");
                return CommandResult.Success;
            }
        }

        private async Task<CommandResult> UpdateContactAsync(Record record, AuthenticationDetail authenticationDetails, PropertiesRequestV1 postData, string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                // When the contact exists we can update the details using https://legacydocs.hubspot.com/docs/methods/contacts/update_contact-by-email
                // It uses the V1 API but support suggests it will be added to V3 before being depreciated so we can use safely:
                // https://community.hubspot.com/t5/APIs-Integrations/Get-Contacts-from-contact-list-using-email/m-p/419493/highlight/true#M41567
                var requestUrl = $"{CrmApiHost}/contacts/v1/contact/email/{email}/profile";
                var response = await GetResponse(requestUrl, HttpMethod.Post, authenticationDetails, postData, JsonContentType).ConfigureAwait(false);
                if (response.IsSuccessStatusCode == false)
                {
                    var retryResult = await HandleFailedRequest(response.StatusCode, requestUrl, HttpMethod.Post, authenticationDetails, postData, JsonContentType);
                    if (retryResult.Success)
                    {
                        _logger.LogInformation($"Hubspot contact record updated from record {record.UniqueId}.");
                        return CommandResult.Success;
                    }
                    else
                    {
                        _logger.LogError("Error updating a HubSpot contact.");
                        return CommandResult.Failed;
                    }
                }
                else
                {
                    _logger.LogInformation($"Hubspot contact record updated from record {record.UniqueId}.");
                    return CommandResult.Success;
                }
            }
            else
            {
                _logger.LogWarning("Could not add a new HubSpot contact due to 409/Conflict response, but no email field was provided to carry out an update.");
                return CommandResult.Failed;
            }
        }

        private AuthenticationDetail GetConfiguredAuthenticationDetails()
        {
            var authentication = new AuthenticationDetail();
            if (TryGetApiKey(out string apiKey))
            {
                authentication.ApiKey = apiKey;
            }
            else if (TryGetPrivateAccessToken(out string privateAccessToken))
            {
                authentication.PrivateAccessToken = privateAccessToken;
            }
            else if (TryGetSavedRefreshToken(out string refreshToken))
            {
                authentication.RefreshToken = refreshToken;
            }

            return authentication;
        }

        private bool TryGetApiKey(out string apiKey)
        {
            apiKey = _settings.ApiKey;

            return !string.IsNullOrEmpty(apiKey);
        }

        private bool TryGetPrivateAccessToken(out string accessToken)
        {
            accessToken = _settings.PrivateAccessToken;

            return !string.IsNullOrEmpty(accessToken);
        }

        private bool TryGetSavedRefreshToken(out string refreshToken)
        {
            refreshToken = _keyValueService.GetValue(RefreshTokenDatabaseKey);
            return !string.IsNullOrEmpty(refreshToken);
        }

        private async Task<string> GetOAuthAccessTokenFromCacheOrRefreshToken(string refreshToken)
        {
            var accessToken = _appCaches.RuntimeCache.Get(AccessTokenCacheKey);
            if (accessToken != null)
            {
                // No access token in the cache, so get a new one from the refresh token.
                await RefreshOAuthAccessToken(refreshToken).ConfigureAwait(false);
                accessToken = _appCaches.RuntimeCache.Get(AccessTokenCacheKey).ToString();
            }

            return accessToken != null ? accessToken.ToString() : string.Empty;
        }

        private async Task RefreshOAuthAccessToken(string refreshToken)
        {
            var tokenRequest = new RefreshTokenRequest
            {
                ClientId = OAuthClientId,
                RedirectUrl = OAuthRedirectUrl,
                RefreshToken = refreshToken,
            };
            var response = await GetResponse(OAuthTokenProxyUrl, HttpMethod.Post, content: tokenRequest, contentType: "application/x-www-form-urlencoded").ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                // Update the token details in the cache.
                _appCaches.RuntimeCache.Insert(AccessTokenCacheKey, () => tokenResponse.AccessToken);

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
            var httpClient = _httpClientFactory.CreateClient();
            
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
                    case AuthenticationMode.PrivateAccessToken:
                        requestMessage.Headers.Authorization =
                           new AuthenticationHeaderValue("Bearer", authenticationDetails.PrivateAccessToken);
                        break;
                    case AuthenticationMode.OAuth:
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", 
                                await GetOAuthAccessTokenFromCacheOrRefreshToken(authenticationDetails.RefreshToken).ConfigureAwait(false));
                        break;
                }
            }

            return await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        }

        private static HttpContent CreateRequestContent(object data, string contentType)
        {
            if (data == null)
            {
                return null;
            }

            switch (contentType)
            {
                case JsonContentType:
                    var serializedData = JsonConvert.SerializeObject(data);
                    return new StringContent(serializedData, Encoding.UTF8, contentType);
                case "application/x-www-form-urlencoded":
                    return new FormUrlEncodedContent(data.AsDictionary());
                default:
                    throw new InvalidOperationException($"Unexpected content type: {contentType}");
            }
        }

        private async Task<HandleFailedRequestResult> HandleFailedRequest(
            HttpStatusCode statusCode,
            string requestUrl,
            HttpMethod httpMethod,
            AuthenticationDetail authenticationDetails,
            object content = null,
            string contentType = "")
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
                    var response = await GetResponse(requestUrl, httpMethod, authenticationDetails, content, contentType).ConfigureAwait(false);
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
