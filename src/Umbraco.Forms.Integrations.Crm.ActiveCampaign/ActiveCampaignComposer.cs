using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Forms.Core.Providers;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Configuration;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign
{
    public class ActiveCampaignComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var options = builder.Services
               .AddOptions<ActiveCampaignSettings>()
               .Bind(builder.Config.GetSection(Constants.SettingsPath));

            builder.WithCollectionBuilder<WorkflowCollectionBuilder>()
                .Add<ActiveCampaignContactsWorkflow>();

            builder.Services
                .AddHttpClient(Constants.HttpClient, client =>
                {
                    client.BaseAddress = new Uri(
                        $"{builder.Config.GetSection(Constants.SettingsPath)[nameof(ActiveCampaignSettings.BaseUrl)]}/api/3/");
                    client.DefaultRequestHeaders
                        .Add("Api-Token", builder.Config.GetSection(Constants.SettingsPath)[nameof(ActiveCampaignSettings.ApiKey)]);
                });

            builder.Services.AddSingleton<IAccountService, AccountService>();
            builder.Services.AddSingleton<IContactService, ContactService>();
        }
    }
}
