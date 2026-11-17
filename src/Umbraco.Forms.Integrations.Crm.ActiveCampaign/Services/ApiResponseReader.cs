using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services
{
    internal static class ApiResponseReader
    {
        /// <summary>
        /// Sends a request and deserializes the body, returning null (and logging a warning) when
        /// ActiveCampaign isn't configured, can't be reached, or doesn't return usable JSON.
        /// The API key and request headers are never logged.
        /// </summary>
        public static async Task<T?> SendAsync<T>(
            HttpClient client, string endpoint, Func<Task<HttpResponseMessage>> send, ILogger logger)
            where T : class
        {
            if (client.BaseAddress is null)
            {
                logger.LogWarning(
                    "ActiveCampaign request to '{Endpoint}' skipped: BaseUrl is not configured under {SettingsPath}.",
                    endpoint, Constants.SettingsPath);
                return null;
            }

            try
            {
                using var response = await send();

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning(
                        "ActiveCampaign request to '{Endpoint}' failed with status code {StatusCode}.",
                        endpoint, (int)response.StatusCode);
                    return null;
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    logger.LogWarning(
                        "ActiveCampaign request to '{Endpoint}' returned an empty response (status code {StatusCode}).",
                        endpoint, (int)response.StatusCode);
                    return null;
                }

                return JsonSerializer.Deserialize<T>(content);
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "ActiveCampaign request to '{Endpoint}' returned invalid JSON.", endpoint);
                return null;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                logger.LogWarning(ex, "ActiveCampaign request to '{Endpoint}' failed.", endpoint);
                return null;
            }
        }
    }
}
