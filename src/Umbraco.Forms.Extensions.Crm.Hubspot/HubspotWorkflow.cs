using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core.Persistence.Dtos;

namespace Umbraco.Forms.Extensions.Crm.Hubspot
{
    public class HubspotWorkflow : WorkflowType
    {
        public HubspotWorkflow()
        {
            Id = new Guid("c47ef1ef-22b1-4b9d-acf6-f57cb8961550");
            Name = "Hubspot";
            Description = "Form submissions are sent to CRM Hubspot";
            Icon = "icon-handshake";
            Group = "CRM";
        }

        [Setting("Hubspot API Key", Description = "Enter the API Key from your HubSpot account", View = "TextField")]
        public string HubspotApiKey { get; set; }

        [Setting("Field Mappings", Description = "Map Umbraco Form fields to HubSpot contact fields", View = "~/App_Plugins/UmbracoFormsExtensions/Hubspot/hubspotfields.html")]
        public string FieldMappings { get; set; }

        private Uri HubspotContactApiUrl
        {
            get
            {
                return new Uri($"https://api.hubapi.com/crm/v3/objects/contacts?hapikey={HubspotApiKey}");
            }
        }

        static readonly HttpClient client = new HttpClient();
        public override WorkflowExecutionStatus Execute(Record record, RecordEventArgs e)
        {
            return WorkflowExecutionStatus.Completed;
        }

        public override List<Exception> ValidateSettings()
        {
            var errors = new List<Exception>();

            // Verify API key is not empty
            if (string.IsNullOrWhiteSpace(HubspotApiKey))
            {
                errors.Add(new Exception("Hubspot API key is missing"));
                return errors;
            }

            // Make a super simple GET request to fetch contacts in HubSpot
            // This way with we can verify that the API key is valid
            // https://developers.hubspot.com/docs/api/crm/contacts
            var testResponse = client.GetAsync(HubspotApiUrl).Result;

            if (testResponse.IsSuccessStatusCode == false)
            {
                // Invalid key will return a 401
                // Message property of response contains useful message
                // The API key provided is invalid. View or manage your API key here: https://app.hubspot.com/l/api-key/
                var errorResponse = testResponse.Content.ReadAsStringAsync().Result;
                var errorObj = JsonConvert.DeserializeObject<ErrorResponse>(errorResponse);
                var ex = new Exception(errorObj.message);
                errors.Add(ex);

                // Log the error
                // TODO: Unable to get Form Name & Form ID for logging context properties
                Current.Logger.Error<HubspotWorkflow>(ex, "Workflow {WorkflowName}: Error checking HubSpot Connection for {FormName} ({FormId})", Workflow.Name);
            }

            return errors;
        }
    }

    public class ErrorResponse
    {
        public string message { get; set; }
    }
}
