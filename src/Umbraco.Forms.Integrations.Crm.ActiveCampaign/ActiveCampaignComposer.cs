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
                    var baseUrl = builder.Config.GetSection(Constants.SettingsPath)[nameof(ActiveCampaignSettings.BaseUrl)];
                    if (!string.IsNullOrWhiteSpace(baseUrl))
                    {
                        client.BaseAddress = new Uri($"{baseUrl}/api/3/");
                        client.DefaultRequestHeaders
                            .Add("Api-Token", builder.Config.GetSection(Constants.SettingsPath)[nameof(ActiveCampaignSettings.ApiKey)]);
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
