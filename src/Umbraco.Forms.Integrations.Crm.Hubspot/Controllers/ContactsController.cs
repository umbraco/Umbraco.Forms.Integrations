using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Forms.Integrations.Crm.Hubspot.Configuration;
using Umbraco.Forms.Integrations.Crm.Hubspot.Extensions;
using Umbraco.Forms.Integrations.Crm.Hubspot.Filters;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models.Responses;
using Umbraco.Forms.Integrations.Crm.Hubspot.Services;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Controllers;

[Route("umbraco/api/hubspot/v1/contacts")]
public class ContactsController : UmbracoApiController
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly ILogger<ContactsController> _logger;
    private readonly AppCaches _appCaches;
    private readonly IKeyValueService _keyValueService;
    private readonly IAuthenticationService _authenticationService;

    private const string CrmApiHost = "https://api.hubapi.com";
    private static readonly string CrmV3ApiBaseUrl = $"{CrmApiHost}/crm/v3/";
    private const string OAuthClientId = "1a04f5bf-e99e-48e1-9d62-6c25bf2bdefe";
    private const string JsonContentType = "application/json";

    private const string OAuthBaseUrl = "https://hubspot-forms-auth.umbraco.com/";  // For local testing: "https://localhost:44364/"
    private static string OAuthRedirectUrl = OAuthBaseUrl;
    private static string OAuthTokenProxyUrl = $"{OAuthBaseUrl}oauth/v1/token";

    public ContactsController(
        IHttpClientFactory httpClientFactory,
        ILogger<ContactsController> logger,
        AppCaches appCaches,
        IKeyValueService keyValueService,
        IAuthenticationService authenticationService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _appCaches = appCaches;
        _keyValueService = keyValueService;
        _authenticationService = authenticationService;
    }

    [HttpGet]
    [ServiceFilter(typeof(ConfiguredAuthenticationFilterAttribute))]
    public async Task<IActionResult> Get()
    {
        var authenticationDetails = _authenticationService.GetDetails();
        var requestUrl = $"{CrmV3ApiBaseUrl}objects/contacts";
        var httpMethod = HttpMethod.Get;
        var response = await GetResponse(requestUrl, httpMethod).ConfigureAwait(false);
        if (response.IsSuccessStatusCode == false)
        {
            var retryResult = await HandleFailedRequest(response.StatusCode, requestUrl, httpMethod, authenticationDetails);
            if (retryResult.Success)
            {
                response = retryResult.RetriedResponse;
            }
            else
            {
                _logger.LogError("Failed to fetch contacts from HubSpot API. {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                return BadRequest(response.ReasonPhrase);
            }
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        return Ok(JsonSerializer.Deserialize<ContactResponse>(responseContent));
    }

    [HttpGet]
    [ServiceFilter(typeof(ConfiguredAuthenticationFilterAttribute))]
    [Route("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var requestUrl = $"{CrmV3ApiBaseUrl}objects/contacts/{id}";
        var httpMethod = HttpMethod.Get;
        var response = await GetResponse(requestUrl, httpMethod).ConfigureAwait(false);
        if (response.IsSuccessStatusCode == false)
        {
            var retryResult = await HandleFailedRequest(response.StatusCode, requestUrl, httpMethod);
            if (retryResult.Success)
            {
                response = retryResult.RetriedResponse;
            }
            else
            {
                _logger.LogError("Failed to fetch contact with id: {id} from HubSpot API. {StatusCode} {ReasonPhrase}", id, response.StatusCode, response.ReasonPhrase);
                return BadRequest(response.ReasonPhrase);
            }
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        return Ok(JsonSerializer.Deserialize<ContactDetail>(responseContent));
    }

    [HttpPost]
    [ServiceFilter(typeof(ConfiguredAuthenticationFilterAttribute))]
    public async Task<IActionResult> Create([FromBody] ContactDetail contact)
    {
        var requestUrl = $"{CrmV3ApiBaseUrl}/objects/contacts";
        var httpMethod = HttpMethod.Post;
        var response = await GetResponse(requestUrl, httpMethod, contact, JsonContentType)
            .ConfigureAwait(false);
        if (response.IsSuccessStatusCode == false)
        {
            var retryResult = await HandleFailedRequest(response.StatusCode, requestUrl, httpMethod);
            if (retryResult.Success)
            {
                response = retryResult.RetriedResponse;
            }
            else
            {
                _logger.LogError("Failed to create contact. {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                return BadRequest(response.ReasonPhrase);
            }
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var contactDetail = JsonSerializer.Deserialize<ContactDetail>(responseContent);
        contact.Id = contactDetail.Id;

        return Created($"contacts/{contactDetail.Id}", contact);
    }

    [HttpPatch]
    [ServiceFilter(typeof(ConfiguredAuthenticationFilterAttribute))]
    [Route("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] ContactDetail contact)
    {
        var requestUrl = $"{CrmV3ApiBaseUrl}/objects/contacts/{id}";
        var httpMethod = HttpMethod.Patch;
        var response = await GetResponse(requestUrl, httpMethod, contact, JsonContentType)
            .ConfigureAwait(false);
        if (response.IsSuccessStatusCode == false)
        {
            var retryResult = await HandleFailedRequest(response.StatusCode, requestUrl, httpMethod);
            if (retryResult.Success)
            {
                response = retryResult.RetriedResponse;
            }
            else
            {
                _logger.LogError("Failed to update contact with id: {id} from HubSpot API. {StatusCode} {ReasonPhrase}", id, response.StatusCode, response.ReasonPhrase);
                return BadRequest(response.ReasonPhrase);
            }
        }

        return Ok();
    }

    [HttpDelete]
    [ServiceFilter(typeof(ConfiguredAuthenticationFilterAttribute))]
    [Route("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var requestUrl = $"{CrmV3ApiBaseUrl}/objects/contacts/{id}";
        var httpMethod = HttpMethod.Delete;
        var response = await GetResponse(requestUrl, httpMethod)
            .ConfigureAwait(false);
        if (response.IsSuccessStatusCode == false)
        {
            var retryResult = await HandleFailedRequest(response.StatusCode, requestUrl, httpMethod);
            if (retryResult.Success)
            {
                response = retryResult.RetriedResponse;
            }
            else
            {
                _logger.LogError("Failed to delete contact with id: {id} from HubSpot API. {StatusCode} {ReasonPhrase}", id, response.StatusCode, response.ReasonPhrase);
                return BadRequest(response.ReasonPhrase);
            }
        }

        return NoContent();
    }

    #region Private Methods

    private async Task<HttpResponseMessage> GetResponse(
            string url,
            HttpMethod httpMethod,
            object content = null,
            string contentType = null)
    {
        var authenticationDetails = _authenticationService.GetDetails();

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
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

            // Update the token details in the cache.
            _appCaches.RuntimeCache.Insert(Constants.AccessTokenCacheKey, () => tokenResponse.AccessToken);

            // Update the saved refresh token if we've got a different one.
            if (tokenResponse.RefreshToken != refreshToken)
            {
                _keyValueService.SetValue(Constants.RefreshTokenDatabaseKey, tokenResponse.RefreshToken);
            }
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            // Failed to refresh an access token from a refresh token, so remove what we have stored.
            // Re-authentication via the authorization code will be required.
            _keyValueService.SetValue(Constants.RefreshTokenDatabaseKey, string.Empty);

            throw new InvalidOperationException(
                $"Could not refresh OAuth token from HubSpot API. Status code: {response.StatusCode}, reason: {response.ReasonPhrase}, content: {responseContent}");
        }
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
                var serializedData = JsonSerializer.Serialize(data);
                return new StringContent(serializedData, Encoding.UTF8, contentType);
            case "application/x-www-form-urlencoded":
                return new FormUrlEncodedContent(data.AsDictionary());
            default:
                throw new InvalidOperationException($"Unexpected content type: {contentType}");
        }
    }

    private async Task<string> GetOAuthAccessTokenFromCacheOrRefreshToken(string refreshToken)
    {
        var accessToken = _appCaches.RuntimeCache.Get(Constants.AccessTokenCacheKey);
        if (accessToken != null)
        {
            // No access token in the cache, so get a new one from the refresh token.
            await RefreshOAuthAccessToken(refreshToken).ConfigureAwait(false);
            accessToken = _appCaches.RuntimeCache.Get(Constants.AccessTokenCacheKey).ToString();
        }

        return accessToken != null ? accessToken.ToString() : string.Empty;
    }

    private async Task<HandleFailedRequestResult> HandleFailedRequest(
            HttpStatusCode statusCode,
            string requestUrl,
            HttpMethod httpMethod,
            object content = null,
            string contentType = "")
    {
        var authenticationDetails = _authenticationService.GetDetails();

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
                var response = await GetResponse(requestUrl, httpMethod, content, contentType).ConfigureAwait(false);
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
        _keyValueService.SetValue(Constants.RefreshTokenDatabaseKey, string.Empty);
    }

    private class HandleFailedRequestResult
    {
        public bool Success { get; set; }

        public HttpResponseMessage RetriedResponse { get; set; }
    }
    #endregion
}
