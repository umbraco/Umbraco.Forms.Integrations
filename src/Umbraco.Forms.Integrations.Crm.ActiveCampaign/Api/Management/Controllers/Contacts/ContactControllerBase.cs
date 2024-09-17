using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Configuration;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Api.Management.Controllers.Contacts
{
    [ApiVersion("1.0")]
    [BackOfficeRoute($"{Constants.ManagementApi.RootPath}/v{{version:apiVersion}}/contacts")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.ContactGroupName)]
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
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
