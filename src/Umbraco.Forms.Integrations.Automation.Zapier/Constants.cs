﻿
namespace Umbraco.Forms.Integrations.Automation.Zapier
{
    public class Constants
    {
        public const string UmbracoFormsIntegrationsAutomationZapierUserGroupAlias = "Umbraco.Forms.Integrations.Automation.Zapier.UserGroupAlias";

        public const string UmbracoFormsIntegrationsAutomationZapierApiKey = "Umbraco.Forms.Integrations.Automation.Zapier.ApiKey";

        public static class ZapierAppConfiguration
        {
            public const string UsernameHeaderKey = "X-USERNAME";

            public const string PasswordHeaderKey = "X-PASSWORD";

            public const string ApiKeyHeaderKey = "X-APIKEY";
        }

        public static class Configuration
        {
            public const string CmsSettings = "Umbraco:CMS:Integrations:Automation:Zapier:Settings";

            public const string FormsSettings = "Umbraco:Forms:Integrations:Automation:Zapier:Settings";
        }

        public static class FormProperties
        {
            public const string Id = "formId";

            public const string Name = "formName";

            public const string SubmissionDate = "submissionDate";

            public const string PageUrl = "pageUrl";
        }

        public static class EntityType
        {
            public const int Form = 2;
        }
    }
}
