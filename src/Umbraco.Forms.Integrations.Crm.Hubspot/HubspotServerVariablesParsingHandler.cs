using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using System;
using System.Collections.Generic;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;
using Umbraco.Forms.Integrations.Crm.Hubspot.Controllers;

namespace Umbraco.Forms.Integrations.Crm.Hubspot
{
    public class HubspotServerVariablesParsingHandler : INotificationHandler<ServerVariablesParsingNotification>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;

        public HubspotServerVariablesParsingHandler(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
        {
            _httpContextAccessor = httpContextAccessor;

            _linkGenerator = linkGenerator; 
        }

        public void Handle(ServerVariablesParsingNotification notification)
        {
            IDictionary<string, object> serverVars = notification.ServerVariables;

            if (!serverVars.ContainsKey("umbracoUrls"))
            {
                throw new ArgumentException("Missing umbracoUrls");
            }

            var umbracoUrlsObject = serverVars["umbracoUrls"];
            if (umbracoUrlsObject == null)
            {
                throw new ArgumentException("Null umbracoUrls");
            }

            if (!(umbracoUrlsObject is Dictionary<string, object?> umbracoUrls))
            {
                throw new ArgumentException("Invalid umbracoUrls");
            }

            if(_httpContextAccessor.HttpContext == null)
            {
                throw new InvalidOperationException("HttpContext is null");
            }

            umbracoUrls["umbracoFormsIntegrationsCrmHubspotBaseUrl"] =
                _linkGenerator.GetUmbracoApiServiceBaseUrl<HubspotController>(controller => controller.GetAllProperties());

        }
    }
}
