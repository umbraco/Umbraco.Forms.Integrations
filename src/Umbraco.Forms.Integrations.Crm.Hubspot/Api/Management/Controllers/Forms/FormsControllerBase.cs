using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Forms.Integrations.Crm.Hubspot.Services;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Api.Management.Controllers.Forms
{
    [ApiVersion("1.0")]
    [BackOfficeRoute($"{Constants.ManagementApi.RootPath}/v{{version:apiVersion}}/forms")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.FormsGroupName)]
    public class FormsControllerBase : HubspotControllerBase
    {
        protected readonly IContactService ContactService;
        public FormsControllerBase(IContactService contactService)
        {
            ContactService = contactService;
        }
    }
}
