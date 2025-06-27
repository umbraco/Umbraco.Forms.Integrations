using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Forms.Integrations.Crm.Hubspot.Services;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Api.Management.Controllers.Contacts
{
    [BackOfficeRoute($"{Constants.ManagementApi.RootPath}/v{{version:apiVersion}}/contacts")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.ContactGroupName)]
    public class ContactControllerBase : HubspotControllerBase
    {
        protected readonly IContactService ContactService;
        public ContactControllerBase(IContactService contactService)
        {
            ContactService = contactService;
        }
    }
}
