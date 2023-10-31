using System.Linq;
using System.Threading.Tasks;

using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;

#if NETCOREAPP
using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
#else
using System.Configuration;
using Umbraco.Core.Services;
#endif

namespace Umbraco.Forms.Integrations.Automation.Zapier.Services
{
    public class UserValidationService : IUserValidationService
    {
        private readonly IUserService _userService;

        private readonly ZapierCmsSettings _zapierCmsSettings;

        private readonly ZapierFormsSettings _zapierFormsSettings;

#if NETCOREAPP
        private readonly IBackOfficeUserManager _backOfficeUserManager;

        public UserValidationService(
            IOptions<ZapierCmsSettings> cmsOptions,
            IOptions<ZapierFormsSettings> formsOptions,
            IBackOfficeUserManager backOfficeUserManager, 
            IUserService userService)
        {
            _backOfficeUserManager = backOfficeUserManager;

            _zapierCmsSettings = cmsOptions.Value;
            _zapierFormsSettings = formsOptions.Value;
        }
#else
        public UserValidationService(IUserService userService)
        {
            _userService = userService;

            _zapierCmsSettings = new ZapierCmsSettings(ConfigurationManager.AppSettings);
            _zapierFormsSettings = new ZapierFormsSettings(ConfigurationManager.AppSettings);
        }
#endif

        /// <summary>
        /// Allow access by validating API Key. If API key is missing, validate user credentials.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public async Task<bool> Validate(string username, string password, string apiKey)
        {
            if (!string.IsNullOrEmpty(apiKey))
            {
                return ValidateByApiKey(apiKey);
            }

            return await ValidateByCredentials(username, password);
        }

        /// <summary>
        /// Validates user based on provided API key. 
        /// When both CMS and Forms packages are installed, both settings (CMS/Forms) will be compared.
        /// </summary>
        /// <param name="apiKey">Provided API key in the Zap authentication.</param>
        /// <returns></returns>
        private bool ValidateByApiKey(string apiKey) =>
            // Check API key from CMS and Forms settings. 
            (!string.IsNullOrEmpty(_zapierCmsSettings.ApiKey) && _zapierCmsSettings.ApiKey == apiKey)
                || (!string.IsNullOrEmpty(_zapierFormsSettings.ApiKey) && _zapierFormsSettings.ApiKey == apiKey);

        /// <summary>
        /// Validates user based on provided credentials.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task<bool> ValidateByCredentials(string username, string password)
        {
#if NETCOREAPP
            var isUserValid =
                await _backOfficeUserManager.ValidateCredentialsAsync(username, password);
#else
            var isUserValid = Umbraco.Web.Composing.Current.UmbracoContext.Security.ValidateBackOfficeCredentials(username, password);
#endif

            if (!isUserValid) return false;

            if (!string.IsNullOrEmpty(_zapierFormsSettings.UserGroupAlias))
            {
                var user = _userService.GetByUsername(username);

                return user != null && user.Groups.Any(p => p.Alias == _zapierFormsSettings.UserGroupAlias);
            }

            return true;
        }

//        public async Task<bool> Validate(string username, string password, string apiKey)
//        {
//            if (!string.IsNullOrEmpty(apiKey))
//                return apiKey == _zapierSettings.ApiKey;

//#if NETCOREAPP
//            var isUserValid =
//                await _backOfficeUserManager.ValidateCredentialsAsync(username, password);
//#else
//            var isUserValid = Umbraco.Web.Composing.Current.UmbracoContext.Security.ValidateBackOfficeCredentials(username, password);
//#endif

//            if (!isUserValid) return false;

//            if (!string.IsNullOrEmpty(_zapierSettings.UserGroupAlias))
//            {
//                var user = _userService.GetByUsername(username);

//                return user != null && user.Groups.Any(p => p.Alias == _zapierSettings.UserGroupAlias);
//            }

//            return true;
//        }
    }
}
