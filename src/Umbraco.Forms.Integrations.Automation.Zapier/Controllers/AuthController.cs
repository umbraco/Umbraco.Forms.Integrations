using System.Configuration;
using System.Linq;


using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;
using Umbraco.Forms.Integrations.Automation.Zapier.Models;


#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
#else
using System.Web.Http;

using Umbraco.Web.WebApi;
using Umbraco.Core.Services;
#endif

namespace Umbraco.Forms.Integrations.Automation.Zapier.Controllers
{
    public class AuthController : UmbracoApiController
    {
        private readonly ZapierSettings Options;

        private readonly IUserService _userService;

#if NETCOREAPP
        private readonly IBackOfficeUserManager _backOfficeUserManager;

        public AuthController(IBackOfficeUserManager backOfficeUserManager, IUserService userService, IOptions<ZapierSettings> options)
        {
            _backOfficeUserManager = backOfficeUserManager;

            _userService = userService;

            Options = options.Value;
        }
#else
        public AuthController(IUserService userService)
        {
            Options = new ZapierSettings(ConfigurationManager.AppSettings);

            _userService = userService;
        }
#endif

        [HttpPost]
        public bool ValidateUser([FromBody] UserModel userModel)
        {
#if NETCOREAPP
            var isUserValid = _backOfficeUserManager.ValidateCredentialsAsync(userModel.Username, userModel.Password).GetAwaiter().GetResult();
#else
            var isUserValid = Security.ValidateBackOfficeCredentials(userModel.Username, userModel.Password);
#endif

            if (!isUserValid) return false;

            var userGroup = Options.UserGroup;
            if (!string.IsNullOrEmpty(userGroup))
            {
                var user = _userService.GetByUsername(userModel.Username);

                return user != null && user.Groups.Any(p => p.Name == userGroup);
            }

            return true;
        }
    }
}
