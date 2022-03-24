using System;
using System.Collections.Generic;
using System.Net.Http;

using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;
using Umbraco.Forms.Integrations.Automation.Zapier.Validators;
using Umbraco.Web;

namespace Umbraco.Forms.Integrations.Automation.Zapier
{
    public class ZapierWorkflow : WorkflowType
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public ZapierWorkflow(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;

            Name = "Trigger Zap";
            Id = new Guid("d05b95e5-86f8-4c31-99b8-4ec7fc62a787");
            Description = "Automation workflow for triggering Zaps in Zapier.";
            Icon = "icon-tools";
        }

        [Core.Attributes.Setting("Fields Mappings",
            Description = "Please map form information to the Zap ones.",
            View = "FieldMapper")]
        public string Mappings { get; set; }

        [Core.Attributes.Setting("Standard Fields Mappings",
            Description = "Please map any standard form information to send.",
            View = "StandardFieldMapper")]
        public string StandardFieldsMappings { get; set; }

        [Core.Attributes.Setting("WebHook Uri", 
            Description = "Zapier WebHook URL",
            View = "TextField")]
        public string WebHookUri { get; set; }

        // Using a static HttpClient (see: https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/).
        private readonly static HttpClient s_client = new HttpClient();

        // Access to the client within the class is via ClientFactory(), allowing us to mock the responses in tests.
        public static Func<HttpClient> ClientFactory = () => s_client;

        public override WorkflowExecutionStatus Execute(Record record, RecordEventArgs e)
        {
            try
            {
                IFieldMappingBuilder builder = new FieldMappingBuilder(_umbracoContextAccessor);

                var content = builder
                    .IncludeFieldsMappings(Mappings, e)
                    .IncludeStandardFieldsMappings(StandardFieldsMappings, e)
                    .Map();

                var result = ClientFactory().PostAsync(WebHookUri, new FormUrlEncodedContent(content)).GetAwaiter().GetResult();

                return result.IsSuccessStatusCode ? WorkflowExecutionStatus.Completed : WorkflowExecutionStatus.Failed;
            }
            catch (Exception exception)
            {
                Umbraco.Core.Composing.Current.Logger.Error(typeof(ZapierWorkflow), exception);

                return WorkflowExecutionStatus.Failed;
            }
        }

        public override List<Exception> ValidateSettings()
        {
            var exceptions = new List<Exception>();

            var validator = new WebHookValidator();
            validator.IsValid(WebHookUri, ref exceptions);

            return exceptions;
        }

        
    }
}
