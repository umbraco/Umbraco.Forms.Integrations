using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Services;
using Umbraco.Forms.Integrations.Crm.Hubspot.Configuration;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly HubspotSettings _settings;

    private readonly IKeyValueService _keyValueService;

	public AuthenticationService(IOptions<HubspotSettings> options, IKeyValueService keyValueService)
	{
		_settings= options.Value;
        _keyValueService= keyValueService;
	}

    public AuthenticationDetail GetDetails()
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
        refreshToken = _keyValueService.GetValue(Constants.RefreshTokenDatabaseKey);
        return !string.IsNullOrEmpty(refreshToken);
    }
}
