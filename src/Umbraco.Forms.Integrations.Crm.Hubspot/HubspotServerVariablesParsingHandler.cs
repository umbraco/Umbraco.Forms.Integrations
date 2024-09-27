using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;
using Umbraco.Forms.Integrations.Crm.Hubspot.Api.Management.Controllers;
using Umbraco.Forms.Integrations.Crm.Hubspot.Configuration;

namespace Umbraco.Forms.Integrations.Crm.Hubspot
{
    public class HubspotServerVariablesParsingHandler : INotificationHandler<ServerVariablesParsingNotification>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;
        private readonly HubspotSettings _settings;

        public HubspotServerVariablesParsingHandler(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator, IOptions<HubspotSettings> options)
        {
            _httpContextAccessor = httpContextAccessor;

            _linkGenerator = linkGenerator;

            _settings = options.Value;
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

            //umbracoUrls["umbracoFormsIntegrationsCrmHubspotBaseUrl"] =
            //    _linkGenerator.GetUmbracoApiServiceBaseUrl<GetAllPropertiesController>(controller => controller.GetAll());

            if (serverVars.ContainsKey("umbracoPlugins"))
            {
                var umbracoPlugins = (Dictionary<string, object>)serverVars["umbracoPlugins"];
                umbracoPlugins.Add("umbracoFormsIntegrationsCrmHubspot", new ClientSideConfiguration
                {
                    AllowContactUpdate = _settings.AllowContactUpdate
                });
            }

        }

        [DataContract]
        internal sealed class ClientSideConfiguration
        {
            [DataMember(Name = "allowContactUpdate")]
            public bool AllowContactUpdate { get; set; }
        }
    }
}
