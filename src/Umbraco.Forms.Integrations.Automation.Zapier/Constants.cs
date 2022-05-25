
namespace Umbraco.Forms.Integrations.Automation.Zapier
{
    public class Constants
    {
        public const string UmbracoFormsIntegrationsAutomationZapierUserGroup = "Umbraco.Forms.Integrations.Automation.Zapier.UserGroup";

        public static class ZapierAppConfiguration
        {
            public const string UsernameHeaderKey = "X-USERNAME";

            public const string PasswordHeaderKey = "X-PASSWORD";
        }

        public static class Configuration
        {
            public const string Settings = "Umbraco:Forms:Integrations:Automation:Zapier:Settings";
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
