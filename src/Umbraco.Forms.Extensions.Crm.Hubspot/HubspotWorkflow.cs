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
        static readonly HttpClient client = new HttpClient();

        private readonly IFacadeConfiguration _configuration;

        public HubspotWorkflow(IFacadeConfiguration configuration)
        {
            _configuration = configuration;

            Id = new Guid("c47ef1ef-22b1-4b9d-acf6-f57cb8961550");
            Name = "Save Contact to Hubspot";
            Description = "Form submissions are sent to Hubspot CRM";
            Icon = "icon-handshake";
            Group = "CRM";
        }

        [Setting("Field Mappings", Description = "Map Umbraco Form fields to HubSpot contact fields", View = "~/App_Plugins/UmbracoFormsExtensions/Hubspot/hubspotfields.html")]
        public string FieldMappings { get; set; }

        public override WorkflowExecutionStatus Execute(Record record, RecordEventArgs e)
        {
            // Check Hubspot key is not empty
            var apiKey = _configuration.GetSetting("HubSpotApiKey");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                // Missing an API Key
                // TODO: Can I bubble up a specific message as to why
                Current.Logger.Warn<HubspotWorkflow>("Workflow {WorkflowName}: No API key has been configurated for the 'Save Contact to HubSpot' the form {FormName} ({FormId})", Workflow.Name, e.Form.Name, e.Form.Id);
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
            var url = $"https://api.hubapi.com/crm/v3/objects/contacts?hapikey={apiKey}";
            var postResponse = client.PostAsync(url, content).GetAwaiter().GetResult();

            // Depending on POST status fail or mark workflow as completed
            if (postResponse.IsSuccessStatusCode == false)
            {
                // LOG THE ERROR
                Current.Logger.Warn<HubspotWorkflow>("Workflow {WorkflowName}: Error submitting data to Hubspot for the form {FormName} ({FormId})", Workflow.Name, e.Form.Name, e.Form.Id);
                return WorkflowExecutionStatus.Failed;
            }

            // TODO:
            // Is it worth logging the success that it got created in HubSpot with its ID etc in response
            // Can get full response with: postResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return WorkflowExecutionStatus.Completed;
        }

        public override List<Exception> ValidateSettings()
        {
            var errors = new List<Exception>();
            return errors;
        }
    }

    internal class PropertiesPost
    {
        public PropertiesPost()
        {
            // Ensure we an init an empty object to add straight away
            Properties = new JObject();
        }

        [JsonProperty(PropertyName = "properties")]
        public JObject Properties { get; set; }
    }

    internal class MappedProperty
    {
        [JsonProperty(PropertyName = "formField")]
        public string FormField { get; set; }

        [JsonProperty(PropertyName = "hubspotField")]
        public string HubspotField { get; set; }
    }

    internal class ErrorResponse
    {
        public string message { get; set; }
    }
}
