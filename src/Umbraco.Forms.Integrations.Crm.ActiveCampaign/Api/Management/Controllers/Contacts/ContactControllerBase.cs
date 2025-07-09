using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Configuration;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Api.Management.Controllers.Contacts
{
    [BackOfficeRoute($"{Constants.ManagementApi.RootPath}/v{{version:apiVersion}}/contacts")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.ContactGroupName)]
    public class ContactControllerBase : ActiveCampaignControllerBase
    {
        protected readonly ActiveCampaignSettings _settings;

        protected readonly IContactService _contactService;

        public ContactControllerBase(IOptions<ActiveCampaignSettings> options, IContactService contactService)
        {
            _settings = options.Value;

            _contactService = contactService;
        }
    }
}
