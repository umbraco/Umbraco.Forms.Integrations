#if NETCOREAPP
using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Forms.Core.Services.Notifications;
using Umbraco.Forms.Integrations.Automation.Zapier.Components;
using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;
using Umbraco.Forms.Integrations.Automation.Zapier.Helpers;
#else
using CSharpTest.Net.Collections;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Forms.Integrations.Automation.Zapier.Helpers;
#endif

using Umbraco.Forms.Integrations.Automation.Zapier.Services;

namespace Umbraco.Forms.Integrations.Automation.Zapier
{
    public class ZapierFormsComposer : IUserComposer
    {
#if NETCOREAPP
        public void Compose(IUmbracoBuilder builder)
        {
            var options = builder.Services
                .AddOptions<ZapierSettings>()
                .Bind(builder.Config.GetSection(Constants.Configuration.Settings));

            builder.AddNotificationHandler<RecordCreatingNotification, NewFormSubmittedNotification>();

            builder.Services.AddSingleton<ZapierFormService>();

            builder.Services.AddSingleton<ZapierFormSubscriptionHookService>();

            builder.Services.AddSingleton<ZapierService>();

            builder.Services.AddScoped<IUserValidationService, UserValidationService>();

            builder.Services.AddSingleton<UmbUrlHelper>();
        }
#else
        public void Compose(Composition composition)
        {
            composition.Register<ZapierFormService>(Lifetime.Singleton);

            composition.Register<ZapierFormSubscriptionHookService>(Lifetime.Singleton);

            composition.Register<ZapierService>(Lifetime.Singleton);

            composition.Register<IUserValidationService, UserValidationService>(Lifetime.Scope);

            composition.Register<UmbUrlHelper>(Lifetime.Singleton);
        }
#endif
    }
}
