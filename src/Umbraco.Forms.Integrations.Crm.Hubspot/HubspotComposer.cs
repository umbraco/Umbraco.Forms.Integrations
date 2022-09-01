using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Forms.Core.Providers;
using Umbraco.Forms.Integrations.Crm.Hubspot.Services;

namespace Umbraco.Forms.Integrations.Crm.Hubspot
{
    public class HubspotComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<IContactService, HubspotContactService>();

            builder.AddNotificationHandler<ServerVariablesParsingNotification, HubspotServerVariablesParsingHandler>();

            builder.WithCollectionBuilder<WorkflowCollectionBuilder>()
                .Add<HubspotWorkflow>();
        }
    }
}
