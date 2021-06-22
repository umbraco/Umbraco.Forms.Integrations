using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
            // Check Hubspot key is not empty
            if (string.IsNullOrWhiteSpace(HubspotApiKey))
            {
                // Missing an API Key
                // TODO: Can I bubble up a specific message as to why
                Current.Logger.Warn<HubspotWorkflow>("Workflow {WorkflowName}: No API key has been set for the Hubspot workflow for the form {FormName} ({FormId})", Workflow.Name, e.Form.Name, e.Form.Id);
                return WorkflowExecutionStatus.NotConfigured;
            }

            var fieldMappingsRawJson = FieldMappings;
            var fieldMappings = JsonConvert.DeserializeObject<List<MappedProperty>>(fieldMappingsRawJson);
            if(fieldMappings.Count == 0)
            {
                Current.Logger.Warn<HubspotWorkflow>("Workflow {WorkflowName}: Missing Hubspot field mappings for workflow for the form {FormName} ({FormId})", Workflow.Name, e.Form.Name, e.Form.Id);
                return WorkflowExecutionStatus.NotConfigured;
            }                

            // Map data from the workflow setting Hubspot fields
            // From the form field values submitted for this form submission
            var postData = new PropertiesPost();

            foreach (var mapping in fieldMappings)
            {
                var fieldId = mapping.FormField;
                var recordField = record.GetRecordField(new Guid(fieldId));
                if (recordField != null)
                {
                    // TODO:
                    // What about different field types in forms & Hubspot that are not simple text ones ?
                    postData.Properties.Add(mapping.HubspotField, recordField.ValuesAsString(false));
                }
                else
                {
                    // There field mapping value could not be found.
                    // Write a warning in the log
                    Current.Logger.Warn<HubspotWorkflow>("Workflow {WorkflowName}: The field mapping with Id, {FieldMappingId}, did not match any record fields. This is probably caused by the record field being marked as sensitive and the workflow has been set not to include sensitive data", Workflow.Name, mapping.FormField);
                }
            }

            // Serialise dynamic JObject to a string for StringContent to POST to URL
            var objAsJson = JsonConvert.SerializeObject(postData);
            var content = new StringContent(objAsJson, Encoding.UTF8, "application/json");

            // POST data to hubspot
            // https://api.hubapi.com/crm/v3/objects/contacts?hapikey=YOUR_HUBSPOT_API_KEY
            var postResponse = client.PostAsync(HubspotContactApiUrl, content).Result;

            // Depending on POST status fail or mark workflow as completed
            if(postResponse.IsSuccessStatusCode == false)
            {
                // LOG THE ERROR
                Current.Logger.Warn<HubspotWorkflow>("Workflow {WorkflowName}: Error submitting data to Hubspot for the form {FormName} ({FormId})", Workflow.Name, e.Form.Name, e.Form.Id);
                return WorkflowExecutionStatus.Failed;
            }

            // TODO:
            // Is it worth logging the success that it got created in HubSpot with its ID etc in response
            var rawResult = postResponse.Content.ReadAsStringAsync().Result;


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
            var testResponse = client.GetAsync(HubspotContactApiUrl).Result;

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

    public class PropertiesPost
    {
        public PropertiesPost()
        {
            // Ensure we an init an empty object to add straight away
            Properties = new JObject();
        }

        [JsonProperty(PropertyName = "properties")]
        public JObject Properties { get; set; }
    }

    public class MappedProperty
    {
        [JsonProperty(PropertyName = "formField")]
        public string FormField { get; set; }

        [JsonProperty(PropertyName = "hubspotField")]
        public string HubspotField { get; set; }
    }

    public class ErrorResponse
    {
        public string message { get; set; }
    }
}
