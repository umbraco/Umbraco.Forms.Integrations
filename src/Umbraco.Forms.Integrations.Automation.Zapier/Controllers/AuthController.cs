﻿using System.Configuration;
using System.Linq;
using System.Web.Http;

using Umbraco.Core.Services;
using Umbraco.Forms.Integrations.Automation.Zapier.Models;
using Umbraco.Web.WebApi;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Controllers
{
    public class AuthController : UmbracoApiController
    {
        private const string UmbracoCmsIntegrationsAutomationZapierUserGroup = "Umbraco.Cms.Integrations.Automation.Zapier.UserGroup";

        [HttpPost]
        public bool ValidateUser([FromBody] UserModel userModel)
        {
            var isUserValid = Security.ValidateBackOfficeCredentials(userModel.Username, userModel.Password);
            if (!isUserValid) return false;

            var userGroup = ConfigurationManager.AppSettings[UmbracoCmsIntegrationsAutomationZapierUserGroup];
            if (!string.IsNullOrEmpty(userGroup))
            {
                IUserService userService = Services.UserService;

                var user = userService.GetByUsername(userModel.Username);

                return user != null && user.Groups.Any(p => p.Name == userGroup);
            }

            return true;
        }
    }
}