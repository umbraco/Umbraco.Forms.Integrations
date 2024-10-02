using Microsoft.Extensions.Logging;
using System.Text.Json;
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

        public const string HubspotSettings = "Umbraco:Forms:Integrations:Crm:Hubspot:Settings";

        private readonly ILogger<HubspotWorkflow> _logger;
        private readonly IContactService _contactService;

        public HubspotWorkflow(ILogger<HubspotWorkflow> logger, IContactService contactService)
        {
            _logger = logger;
            _contactService = contactService;

            Id = new Guid(HubspotWorkflowId);
            Name = "Save Contact to HubSpot";
            Description = "Form submissions are sent to HubSpot CRM";
            Icon = "icon-handshake";
            Group = "CRM";
        }

        [Setting("Field Mappings", Description = "Map Umbraco Form fields to HubSpot contact fields", View = "Hubspot.PropertyEditorUi.Mapping")]
        public string FieldMappings { get; set; }

        public override async Task<WorkflowExecutionStatus> ExecuteAsync(WorkflowExecutionContext context)
        {
            var fieldMappingsRawJson = FieldMappings;
            var fieldMappings = JsonSerializer.Deserialize<List<MappedProperty>>(fieldMappingsRawJson);
            if (fieldMappings.Count == 0)
            {
                _logger.LogWarning("Save Contact to HubSpot: Missing HubSpot field mappings for workflow for the form {FormName} ({FormId})", context.Form.Name, context.Form.Id);
                return WorkflowExecutionStatus.NotConfigured;
            }

            var commandResult = await _contactService.PostContactAsync(context.Record, fieldMappings, null);
            switch (commandResult)
            {
                case CommandResult.NotConfigured:
                    _logger.LogWarning("Save Contact to HubSpot: Could not complete contact request for {FormName} ({FormId}) as the workflow is not correctly configured.", context.Form.Name, context.Form.Id);
                    return WorkflowExecutionStatus.NotConfigured;
                case CommandResult.Failed:
                    _logger.LogWarning("Save Contact to HubSpot: Failed for {FormName} ({FormId}).", context.Form.Name, context.Form.Id);
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
