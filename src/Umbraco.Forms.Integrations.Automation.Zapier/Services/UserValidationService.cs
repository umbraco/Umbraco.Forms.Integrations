using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Services
{
    public class UserValidationService : IUserValidationService
    {
        private readonly ZapierSettings _zapierSettings;

        private readonly IUserService _userService;

        private readonly IBackOfficeUserManager _backOfficeUserManager;

        public UserValidationService(IOptions<ZapierSettings> options, IUserService userService, IBackOfficeUserManager backOfficeUserManager)
        {
            _zapierSettings = options.Value;

            _backOfficeUserManager = backOfficeUserManager;

            _userService = userService;
        }

        public async Task<bool> Validate(string username, string password, string apiKey)
        {
            if (!string.IsNullOrEmpty(apiKey))
                return apiKey == _zapierSettings.ApiKey;

            var isUserValid =
                await _backOfficeUserManager.ValidateCredentialsAsync(username, password);

            if (!isUserValid) return false;

            if (!string.IsNullOrEmpty(_zapierSettings.UserGroupAlias))
            {
                var user = _userService.GetByUsername(username);

                return user != null && user.Groups.Any(p => p.Alias == _zapierSettings.UserGroupAlias);
            }

            return true;
        }
    }
}
