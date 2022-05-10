#if NETCOREAPP
using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;
#else
using Umbraco.Core;
using Umbraco.Core.Composing;
#endif

using Umbraco.Forms.Integrations.Automation.Zapier.Services;

namespace Umbraco.Forms.Integrations.Automation.Zapier
{
    public class ZapierFormsComposer : IComposer
    {
#if NETCOREAPP
        public void Compose(IUmbracoBuilder builder)
        {
            var options = builder.Services
                .AddOptions<ZapierSettings>()
                .Bind(builder.Config.GetSection(Constants.Configuration.Settings));

            builder.Services.AddSingleton<ZapierService>();
            
            builder.Services.AddSingleton<ZapierFormSubscriptionHookService>();
        }
#else
        public void Compose(Composition composition)
        {
            composition.Register<ZapierService>(Lifetime.Singleton);

            composition.Register<ZapierFormSubscriptionHookService>(Lifetime.Singleton);
        }
#endif
    }
}
