using System;
using System.Collections.Generic;
using Umbraco.Forms.Core;
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

        public override WorkflowExecutionStatus Execute(Record record, RecordEventArgs e)
        {
            return WorkflowExecutionStatus.Completed;
        }

        public override List<Exception> ValidateSettings()
        {
            return new List<Exception>();
        }
    }
}
