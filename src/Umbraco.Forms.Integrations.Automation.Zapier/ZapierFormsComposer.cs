using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Forms.Core.Services.Notifications;
using Umbraco.Forms.Integrations.Automation.Zapier.Components;
using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;
using Umbraco.Forms.Integrations.Automation.Zapier.Helpers;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

namespace Umbraco.Forms.Integrations.Automation.Zapier
{
    public class ZapierFormsComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services
                .AddOptions<ZapierCmsSettings>()
                .Bind(builder.Config.GetSection(Constants.Configuration.CmsSettings));
            builder.Services
                .AddOptions<ZapierFormsSettings>()
                .Bind(builder.Config.GetSection(Constants.Configuration.Settings));

            builder
                .AddNotificationAsyncHandler<RecordCreatingNotification, NewFormSubmittedNotification>();

            builder.Services.AddSingleton<ZapierFormService>();

            builder.Services.AddSingleton<ZapierFormSubscriptionHookService>();

            builder.Services.AddSingleton<ZapierService>();

            builder.Services.AddScoped<IUserValidationService, UserValidationService>();

            builder.Services.AddSingleton<UmbUrlHelper>();

            builder.AddBackOfficeOpenApiDocument(
                Constants.ManagementApi.ApiName,
                document => document
                    .WithTitle(Constants.ManagementApi.ApiTitle)
                    .WithBackOfficeAuthentication()
                    .ConfigureOpenApiOptions(options => options.AddDocumentTransformer((doc, _, _) =>
                    {
                        doc.Info.Version = "Latest";
                        doc.Info.Description = $"Describes the {Constants.ManagementApi.ApiTitle} available for handling Zapier Forms automation and configuration.";
                        return Task.CompletedTask;
                    })));
        }
    }
}
