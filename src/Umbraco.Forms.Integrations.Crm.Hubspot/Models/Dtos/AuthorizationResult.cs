using Newtonsoft.Json;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Models.Dtos
{
    public class AuthorizationResult
    {
        private AuthorizationResult()
        {
        }

        [JsonProperty("success")]
        public bool Success { get; private set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; private set; }

        public static AuthorizationResult AsSuccess() =>
            new AuthorizationResult { Success = true };

        public static AuthorizationResult AsError(string message) =>
            new AuthorizationResult { ErrorMessage = message };

    }
}
