#if NETCOREAPP
using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;
#else
using CSharpTest.Net.Collections;
using Umbraco.Core;
using Umbraco.Core.Composing;
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

            builder.Services.AddSingleton<ZapierFormService>();

            builder.Services.AddSingleton<ZapierFormSubscriptionHookService>();

            builder.Services.AddSingleton<ZapierService>();

            builder.Services.AddScoped<IUserValidationService, UserValidationService>();
        }
#else
        public void Compose(Composition composition)
        {
            composition.Register<ZapierFormService>(Lifetime.Singleton);

            composition.Register<ZapierFormSubscriptionHookService>(Lifetime.Singleton);

            composition.Register<ZapierService>(Lifetime.Singleton);

            composition.Register<IUserValidationService, UserValidationService>(Lifetime.Scope);
        }
#endif
    }
}
