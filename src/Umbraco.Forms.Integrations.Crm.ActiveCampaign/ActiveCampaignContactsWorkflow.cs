using Microsoft.Extensions.Options;

using System.Text.Json;

using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Configuration;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign
{
    public class ActiveCampaignContactsWorkflow : WorkflowType
    {
        private readonly ActiveCampaignSettings _settings;

        private readonly IContactService _contactService;

        [Core.Attributes.Setting("Contact Mappings",
            Description = "Map contact details with form fields",
            View = "~/App_Plugins/UmbracoForms.Integrations/Crm/ActiveCampaign/contact-mapper.html")]
        public string ContactMappings { get; set; }

        public ActiveCampaignContactsWorkflow(IOptions<ActiveCampaignSettings> options, IContactService contactService)
        {
            Id = new Guid(Constants.WorkflowId);
            Name = "ActiveCampaign Contacts Workflow";
            Description = "Submit form data to ActiveCampaign Contacts";
            Icon = "icon-users";

            _settings = options.Value;

            _contactService = contactService;
        }

        public override WorkflowExecutionStatus Execute(WorkflowExecutionContext context)
        {
            var mappings = JsonSerializer.Deserialize<List<ContactMappingDto>>(ContactMappings);

            var email = context.Record.RecordFields[Guid.Parse(mappings.First(p => p.ContactField == "email").FormField.Id)]
                .ValuesAsString();

            var contacts = _contactService.Get(email).Result;

            var requestDto = new ContactRequestDto { Contact = Build(context.Record) };
            if (contacts.Contacts.Count > 0) requestDto.Contact.Id = contacts.Contacts.First().Id;

            var result = _contactService.CreateOrUpdate(requestDto, contacts.Contacts.Count > 0).Result;

            if (!result) return WorkflowExecutionStatus.Failed;

            return WorkflowExecutionStatus.Completed;
        }

        public override List<Exception> ValidateSettings()
        {
            var list = new List<Exception>();

            if (string.IsNullOrEmpty(ContactMappings))
                list.Add(new Exception("Contact mappings are required."));

            var mappings = JsonSerializer.Deserialize<List<ContactMappingDto>>(ContactMappings);
            bool validMappings = _settings.ContactFields
                .Where(p => p.Required)
                .Any(p => mappings.Select(q => q.ContactField).Contains(p.Name));
            if (!validMappings)
                list.Add(new Exception("Invalid contact mappings. Please make sure the mandatory fields are mapped."));

            return list;
        }

        /// <summary>
        /// Create Contact instance using the mapped details and record fields
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private ContactDto Build(Record record)
        {
            var mappings = JsonSerializer.Deserialize<List<ContactMappingDto>>(ContactMappings);

            return new ContactDto
            {
                Email = ReadMappingValue(record, mappings, "email"),
                FirstName = ReadMappingValue(record, mappings, "firstName"),
                LastName = ReadMappingValue(record, mappings, "lastName"),
                Phone = ReadMappingValue(record, mappings, "phone")
            };
        }

        private string ReadMappingValue(Record record, List<ContactMappingDto> mappings, string name)
        {
            var mappingItem = mappings.FirstOrDefault(p => p.ContactField == name);

            return mappingItem != null
                ? record.RecordFields[Guid.Parse(mappingItem.FormField.Id)].ValuesAsString()
                : string.Empty;
        }
    }
}
