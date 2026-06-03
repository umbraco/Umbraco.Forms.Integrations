using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
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

            builder.AddBackOfficeOpenApiDocument(
                Constants.ManagementApi.ApiName,
                document => document
                    .WithTitle(Constants.ManagementApi.ApiTitle)
                    .WithBackOfficeAuthentication()
                    .ConfigureOpenApiOptions(openApiOptions => openApiOptions.AddDocumentTransformer((doc, _, _) =>
                    {
                        doc.Info.Version = "Latest";
                        doc.Info.Description = $"Describes the {Constants.ManagementApi.ApiTitle} available for handling ActiveCampaign automation and configuration.";
                        return Task.CompletedTask;
                    })));
        }
    }
}
