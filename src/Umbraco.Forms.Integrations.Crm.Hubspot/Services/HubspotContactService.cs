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
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Integrations.Crm.Hubspot.Extensions;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models;
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

        private const string CrmApiBaseUrl = "https://api.hubapi.com/crm/v3/";
        private const string AuthApiTokenUrl = "https://api.hubapi.com/oauth/v1/token";
        private const string OAauthClientId = "";
        private const string OAauthSecret = "";
        private const string OAuthRedirectUrl = "https://localhost:44364/";

        private const string AccessTokenCacheKey = "HubSpotOAuthAccessToken";

        public HubspotContactService(IFacadeConfiguration configuration, ILogger logger, AppCaches appCaches)
        {
            _configuration = configuration;
            _logger = logger;
            _appCaches = appCaches;
        }

        public async Task<IEnumerable<Property>> GetContactProperties()
        {
            if (!TryGetConfiguredAuthenticationDetails(out HubspotAuthentication authenticationDetails))
            {
                return Enumerable.Empty<Property>();
            }

            var url = ConstructUrl("properties/contacts", authenticationDetails.ApiKey);
            var response = await GetResponse(url, HttpMethod.Get, authenticationDetails).ConfigureAwait(false);
            if (response.IsSuccessStatusCode == false)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized && authenticationDetails.Mode == HubspotAuthenticationMode.OAuth)
                {
                    // If we've got a 403 response and are using OAuth, likely our access token has expired.
                    // First we should try to refresh it using the refresh token.  If successful this will save the new
                    // value into the cache.
                    await RefreshOAuthAccessToken();

                    // Repeat the operation using either the refreshed token or a newly requested one.
                    response = await GetResponse(url, HttpMethod.Get, authenticationDetails).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode == false)
                    {
                        _logger.Error<HubspotContactService>("Failed to fetch contact properties from HubSpot API for mapping. {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                        return Enumerable.Empty<Property>();
                    }
                }

                _logger.Error<HubspotContactService>("Failed to fetch contact properties from HubSpot API for mapping. {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                return Enumerable.Empty<Property>();
            }

            // Map the properties to our simpler object, as we don't need all the fields in the response.
            var properties = new List<Property>();
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseContentAsJson = JsonConvert.DeserializeObject<PropertiesResponse>(responseContent);
            properties.AddRange(responseContentAsJson.Results);
            return properties.OrderBy(x => x.Label);
        }

        public async Task<CommandResult> PostContact(Record record, List<MappedProperty> fieldMappings)
        {
            if (!TryGetConfiguredAuthenticationDetails(out HubspotAuthentication authenticationDetails))
            {
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
            var url = ConstructUrl("objects/contacts", authenticationDetails.ApiKey);
            var response = await GetResponse(url, HttpMethod.Post, authenticationDetails, postData, "application/json").ConfigureAwait(false);

            // Depending on POST status fail or mark workflow as completed
            if (response.IsSuccessStatusCode == false)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized && authenticationDetails.Mode == HubspotAuthenticationMode.OAuth)
                {
                    // If we've got a 403 response and are using OAuth, likely our access token has expired.
                    // First we should try to refresh it using the refresh token.  If successful this will save the new
                    // value into the cache.
                    await RefreshOAuthAccessToken();

                    // Repeat the operation using either the refreshed token or a newly requested one.
                    response = await GetResponse(url, HttpMethod.Post, authenticationDetails, postData, "application/json").ConfigureAwait(false);
                    if (response.IsSuccessStatusCode == false)
                    {
                        _logger.Error<HubspotContactService>("Error submitting a HubSpot contact request ");
                        return CommandResult.Failed;
                    }
                }

                _logger.Error<HubspotContactService>("Error submitting a HubSpot contact request ");
                return CommandResult.Failed;
            }

            return CommandResult.Success;
        }

        private bool TryGetConfiguredAuthenticationDetails(out HubspotAuthentication authentication)
        {
            authentication = new HubspotAuthentication(GetAuthenticationMode());

            switch(authentication.Mode)
            {
                case HubspotAuthenticationMode.ApiKey:
                    if (!TryGetApiKey(out string apiKey))
                    {
                        _logger.Warn<HubspotContactService>("Cannot access HubSpot API as although the authentication mode is configured to use an API Key, no key has been configured.");
                        return false;
                    }

                    authentication.ApiKey = apiKey;
                    break;
                case HubspotAuthenticationMode.OAuth:
                    if (!TryGetOAuthAuthorizationCode(out string oauthAuthorizationCode))
                    {
                        _logger.Warn<HubspotContactService>("Cannot access HubSpot API as although the authentication mode is configured to use OAuth, no authorization code has been configured.");
                        return false;
                    }

                    authentication.OAuthAuthenticationCode = oauthAuthorizationCode;
                    break;
                default:
                    _logger.Warn<HubspotContactService>("Cannot access HubSpot API as no or an unrecognized authentication mode has been configured.");
                    return false;
            }

            return true;
        }

        private HubspotAuthenticationMode GetAuthenticationMode()
        {
            var configValue = _configuration.GetSetting("HubSpotAuthenticationMode");
            if (string.IsNullOrEmpty(configValue))
            {
                return HubspotAuthenticationMode.Undefined;
            }

            if (Enum.TryParse(configValue, out HubspotAuthenticationMode authenticationMode))
            {
                return authenticationMode;
            }

            return HubspotAuthenticationMode.Undefined;
        }

        private bool TryGetApiKey(out string apiKey)
        {
            apiKey = _configuration.GetSetting("HubSpotApiKey");
            return !string.IsNullOrEmpty(apiKey);
        }

        private bool TryGetOAuthAuthorizationCode(out string authorizationCode)
        {
            authorizationCode = _configuration.GetSetting("HubSpotOAuthAuthorizationCode");
            return !string.IsNullOrEmpty(authorizationCode);
        }

        private async Task<string> GetOAuthAccessToken(string authorizationCode)
        {
            var tokenResponse = _appCaches.RuntimeCache.GetCacheItem<TokenResponse>(AccessTokenCacheKey);
            if (tokenResponse == null)
            { 
                var tokenRequest = new GetTokenRequest
                {
                    ClientId = OAauthClientId,
                    ClientSecret = OAauthSecret,
                    RedirectUrl = OAuthRedirectUrl,
                    AuthorizationCode = authorizationCode,
                };
                var response = await GetResponse(AuthApiTokenUrl, HttpMethod.Post, content: tokenRequest, contentType: "application/x-www-form-urlencoded").ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    tokenResponse = await response.Content.ReadAsAsync<TokenResponse>();

                    // Add the token details to the cache.
                    _appCaches.RuntimeCache.InsertCacheItem(AccessTokenCacheKey, () => tokenResponse);
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException(
                        $"Could not retrieve OAuth token from HubSpot API. Status code: {response.StatusCode}, reason: {response.ReasonPhrase}, content: {responseContent}");
                }
            }

            return tokenResponse.AccessToken;
        }

        private async Task RefreshOAuthAccessToken()
        {
            var tokenResponse = _appCaches.RuntimeCache.GetCacheItem<TokenResponse>(AccessTokenCacheKey);
            if (tokenResponse == null)
            {
                return;
            }

            var tokenRequest = new RefreshTokenRequest
            {
                ClientId = OAauthClientId,
                ClientSecret = OAauthSecret,
                RedirectUrl = OAuthRedirectUrl,
                RefreshToken = tokenResponse.RefreshToken,
            };
            var response = await GetResponse(AuthApiTokenUrl, HttpMethod.Post, content: tokenRequest, contentType: "application/x-www-form-urlencoded").ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                tokenResponse = await response.Content.ReadAsAsync<TokenResponse>();

                // Update the token details in the cache.
                _appCaches.RuntimeCache.InsertCacheItem(AccessTokenCacheKey, () => tokenResponse);
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Could not refresh OAuth token from HubSpot API. Status code: {response.StatusCode}, reason: {response.ReasonPhrase}, content: {responseContent}");
            }
        }

        private string ConstructUrl(string path, string apiKey)
        {
            var url = $"{CrmApiBaseUrl}{path}";
            if (!string.IsNullOrEmpty(apiKey))
            {
                url += "?hapikey={apiKey}";
            }

            return url;            
        }

        private async Task<HttpResponseMessage> GetResponse(
            string url,
            HttpMethod httpMethod,
            HubspotAuthentication authenticationDetails = null,
            object content = null,
            string contentType = null)
        {
            var requestMessage = new HttpRequestMessage
            {
                Method = httpMethod,
                RequestUri = new Uri(url),
                Content = CreateRequestContent(content, contentType),
            };

            if (authenticationDetails != null && authenticationDetails.Mode == HubspotAuthenticationMode.OAuth)
            {
                requestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", await GetOAuthAccessToken(authenticationDetails.OAuthAuthenticationCode));
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
    }
}
