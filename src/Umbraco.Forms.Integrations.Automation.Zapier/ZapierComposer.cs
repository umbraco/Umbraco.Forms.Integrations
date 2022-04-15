#if NETCOREAPP
using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Forms.Core.Providers;
using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;
#else
using Umbraco.Core.Composing;
#endif
using Umbraco.Core;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

namespace Umbraco.Forms.Integrations.Automation.Zapier
{
    public class ZapierComposer : IComposer
    {
#if NETCOREAPP
        public void Compose(IUmbracoBuilder builder)
        {
            var options = builder.Services
                .AddOptions<ZapierSettings>()
                .Bind(builder.Config.GetSection(Constants.Configuration.Settings));

            builder.WithCollectionBuilder<WorkflowCollectionBuilder>().Add<ZapierWorkflow>();

            builder.Services.AddScoped<IUserValidationService, UserValidationService>();
        }
#else
        public void Compose(Composition composition)
        {
            composition.Register<IUserValidationService, UserValidationService>(Lifetime.Scope);
        }
#endif
    }
}
