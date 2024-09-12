using Microsoft.Extensions.Logging;
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

        private readonly IAccountService _accountService;

        private readonly IContactService _contactService;

        private readonly ILogger<ActiveCampaignContactsWorkflow> _logger;

        [Core.Attributes.Setting("Account",
            Description = "Please select an account",
            View = "~/App_Plugins/UmbracoForms.Integrations/Crm/ActiveCampaign/accountpicker.html")]
        public string Account { get; set; }

        [Core.Attributes.Setting("Contact Mappings",
            Description = "Map contact details with form fields",
            View = "~/App_Plugins/UmbracoForms.Integrations/Crm/ActiveCampaign/contact-mapper.html")]
        public string ContactMappings { get; set; }

        [Core.Attributes.Setting("Custom Field Mappings",
            Description = "Map contact custom fields with form fields",
            View = "~/App_Plugins/UmbracoForms.Integrations/Crm/ActiveCampaign/customfield-mapper.html")]
        public string CustomFieldMappings { get; set; }

        public ActiveCampaignContactsWorkflow(IOptions<ActiveCampaignSettings> options, 
            IAccountService accountService, IContactService contactService,
            ILogger<ActiveCampaignContactsWorkflow> logger)
        {
            Id = new Guid(Constants.WorkflowId);
            Name = "ActiveCampaign Contacts Workflow";
            Description = "Submit form data to ActiveCampaign Contacts";
            Icon = "icon-users";

            _settings = options.Value;

            _accountService = accountService;

            _contactService = contactService;

            _logger = logger;
        }

        public override async Task<WorkflowExecutionStatus> ExecuteAsync(WorkflowExecutionContext context)
        {
            try
            {
                var mappings = JsonSerializer.Deserialize<List<ContactMappingDto>>(ContactMappings);

                var email = context.Record.RecordFields[Guid.Parse(mappings.First(p => p.ContactField == "email").FormField.Id)]
                    .ValuesAsString();

                // Check if contact exists.
                var contacts = _contactService.Get(email).ConfigureAwait(false).GetAwaiter().GetResult();

                if(contacts.Contacts.Count > 0 && !_settings.AllowContactUpdate)
                {
                    _logger.LogInformation("Contact already exists in ActiveCampaign and workflow is configured to not apply updates, so update of information was skipped.");

                    return await Task.FromResult<WorkflowExecutionStatus>(WorkflowExecutionStatus.Completed);
                }

                var requestDto = new ContactDetailDto { Contact = Build(context.Record) };

                if (contacts.Contacts.Count > 0) requestDto.Contact.Id = contacts.Contacts.First().Id;

                // Set contact custom fields.
                if (!string.IsNullOrEmpty(CustomFieldMappings))
                {
                    var customFieldMappings = JsonSerializer.Deserialize<List<CustomFieldMappingDto>>(CustomFieldMappings);

                    requestDto.Contact.FieldValues = customFieldMappings.Select(p => new CustomFieldValueDto
                    {
                        Field = p.CustomField.Id,
                        Value = context.Record.RecordFields[Guid.Parse(p.FormField.Id)].ValuesAsString()
                    }).ToList();
                }

                var contactId = _contactService.CreateOrUpdate(requestDto, contacts.Contacts.Count > 0)
                    .ConfigureAwait(false).GetAwaiter().GetResult();

                if (string.IsNullOrEmpty(contactId))
                {
                    _logger.LogError($"Failed to create/update contact: {email}");

                    return await Task.FromResult<WorkflowExecutionStatus>(WorkflowExecutionStatus.Failed);
                }

                // Associate contact with account if last one is specified.
                if (!string.IsNullOrEmpty(Account))
                {
                    var associationResponse = _accountService.CreateAssociation(int.Parse(Account), int.Parse(contactId))
                        .ConfigureAwait(false).GetAwaiter().GetResult();
                }

                return await Task.FromResult<WorkflowExecutionStatus>(WorkflowExecutionStatus.Completed);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return await Task.FromResult<WorkflowExecutionStatus>(WorkflowExecutionStatus.Failed);
            }
        }

        public override List<Exception> ValidateSettings()
        {
            var list = new List<Exception>();

            if (string.IsNullOrEmpty(ContactMappings))
                list.Add(new Exception("Contact mappings are required."));

            var mappings = JsonSerializer.Deserialize<List<ContactMappingDto>>(ContactMappings);
            foreach(var contactField in _settings.ContactFields.Where(p => p.Required))
            {
                if(!mappings.Any(p => p.ContactField == contactField.Name))
                {
                    list.Add(new Exception("Invalid contact mappings. Please make sure the mandatory fields are mapped."));
                    break;
                }
            }

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
