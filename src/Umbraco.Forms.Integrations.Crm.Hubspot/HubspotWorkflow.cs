using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;

using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models;
using Umbraco.Forms.Integrations.Crm.Hubspot.Services;

namespace Umbraco.Forms.Integrations.Crm.Hubspot
{
    public class HubspotWorkflow : WorkflowType
    {
        public const string HubspotWorkflowId = "c47ef1ef-22b1-4b9d-acf6-f57cb8961550";

        public const string HubspotApiKey = "Umbraco:Forms:Integrations:Crm:Hubspot:Settings:ApiKey";

        private readonly ILogger<HubspotWorkflow> _logger;
        private readonly IContactService _contactService;

        public HubspotWorkflow(ILogger<HubspotWorkflow> logger, IContactService contactService)
        {
            _logger = logger;
            _contactService = contactService;

            Id = new Guid(HubspotWorkflowId);
            Name = "Save Contact to Hubspot";
            Description = "Form submissions are sent to Hubspot CRM";
            Icon = "icon-handshake";
            Group = "CRM";
        }

        [Setting("Field Mappings", Description = "Map Umbraco Form fields to HubSpot contact fields", View = "~/App_Plugins/UmbracoForms.Integrations/Crm/Hubspot/hubspotfields.html")]
        public string FieldMappings { get; set; }

        public override WorkflowExecutionStatus Execute(WorkflowExecutionContext context)
        {
            var fieldMappingsRawJson = FieldMappings;
            var fieldMappings = JsonConvert.DeserializeObject<List<MappedProperty>>(fieldMappingsRawJson);
            if (fieldMappings.Count == 0)
            {
                _logger.LogWarning("Workflow {WorkflowName}: Missing Hubspot field mappings for workflow for the form {FormName} ({FormId})", 
                    Workflow.Name, context.Form.Name, context.Form.Id);
                return WorkflowExecutionStatus.NotConfigured;
            }

            var commandResult = _contactService.PostContactAsync(context.Record, fieldMappings, null).GetAwaiter().GetResult();
            switch (commandResult)
            {
                case CommandResult.NotConfigured:
                    _logger.LogWarning("Workflow {WorkflowName}: Could not complete contact request for {FormName} ({FormId}) as the workflow is not correctly configured.",
                        Workflow.Name, context.Form.Name, context.Form.Id);
                    return WorkflowExecutionStatus.NotConfigured;
                case CommandResult.Failed:
                    _logger.LogWarning("Workflow {WorkflowName}: Failed for {FormName} ({FormId}).", Workflow.Name, context.Form.Name, context.Form.Id);
                    return WorkflowExecutionStatus.Failed;
                case CommandResult.Success:
                    return WorkflowExecutionStatus.Completed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(commandResult));
            }
        }

        public override List<Exception> ValidateSettings() => new List<Exception>();
    }
}
