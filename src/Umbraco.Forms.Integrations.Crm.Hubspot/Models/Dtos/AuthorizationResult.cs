using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Models.Dtos
{
    public class AuthorizationResult
    {
        private AuthorizationResult()
        {
        }

        [JsonPropertyName("success")]
        public bool Success { get; private set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; private set; }

        public static AuthorizationResult AsSuccess() =>
            new AuthorizationResult { Success = true };

        public static AuthorizationResult AsError(string message) =>
            new AuthorizationResult { ErrorMessage = message };

    }
}
