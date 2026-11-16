using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Forms.Core.Providers;
using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Api.Configuration;
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
                    // Leave BaseAddress unset when not configured; the services treat that as "not configured".
                    var settingsSection = builder.Config.GetSection(Constants.SettingsPath);
                    var baseUrl = settingsSection[nameof(ActiveCampaignSettings.BaseUrl)];
                    var apiKey = settingsSection[nameof(ActiveCampaignSettings.ApiKey)];
                    if (ApiConfiguration.IsValid(baseUrl, apiKey) && ApiConfiguration.TryGetApiBaseAddress(baseUrl, out var baseAddress))
                    {
                        client.BaseAddress = baseAddress;
                        client.DefaultRequestHeaders.Add("Api-Token", apiKey);
                    }
                });

            builder.Services.AddSingleton<IAccountService, AccountService>();
            builder.Services.AddSingleton<IContactService, ContactService>();

            // Generate Swagger documentation for Zapier API
            builder.Services.Configure<SwaggerGenOptions>(options =>
            {
                options.SwaggerDoc(
                    Constants.ManagementApi.ApiName,
                    new OpenApiInfo
                    {
                        Title = Constants.ManagementApi.ApiTitle,
                        Version = "Latest",
                        Description = $"Describes the {Constants.ManagementApi.ApiTitle} available for handling ActiveCampaign automation and configuration."
                    });

                options.OperationFilter<BackOfficeSecurityRequirementsOperationFilter>();
            })
            .AddSingleton<IOperationIdHandler, ActiveCampaignOperationIdHandler>();
        }
    }
}
