using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Forms.Integrations.Automation.Zapier.Configuration;
using Umbraco.Forms.Integrations.Automation.Zapier.Helpers;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

namespace Umbraco.Forms.Integrations.Automation.Zapier
{
    public class ZapierFormsComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var options = builder.Services
                .AddOptions<ZapierSettings>()
                .Bind(builder.Config.GetSection(Constants.Configuration.Settings));

            builder.Services.AddSingleton<ZapierFormService>();

            builder.Services.AddSingleton<ZapierFormSubscriptionHookService>();

            builder.Services.AddSingleton<ZapierService>();

            builder.Services.AddScoped<IUserValidationService, UserValidationService>();

            builder.Services.AddSingleton<UmbUrlHelper>();
        }
    }
}
