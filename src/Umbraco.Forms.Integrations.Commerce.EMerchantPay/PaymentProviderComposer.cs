#if NETCOREAPP
using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Forms.Core.Providers;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Configuration;

#else
using Umbraco.Core;
using Umbraco.Core.Composing;
#endif

using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Models.Dtos;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Helpers;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Services;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay
{
    public class PaymentProviderComposer : IComposer
    {
#if NETCOREAPP
        public void Compose(IUmbracoBuilder builder)
        {
            var options = builder.Services
                .AddOptions<PaymentProviderSettings>()
                .Bind(builder.Config.GetSection(Constants.Configuration.Settings));

            builder.WithCollectionBuilder<WorkflowCollectionBuilder>()
                .Add<PaymentProviderWorkflow>();

            builder.Services.AddSingleton<ConsumerService>();

            builder.Services.AddSingleton<PaymentService>();

            builder.Services.AddSingleton<UrlHelper>();

            builder.Services.AddSingleton<CurrencyHelper>();

            builder.Services.AddSingleton<MappingFieldHelper>();

            builder.Services.AddSingleton<IMappingService<Mapping>, MappingService>();
        }
#else
        public void Compose(Composition composition)
        {
            composition.Register<ConsumerService>(Lifetime.Singleton);

            composition.Register<PaymentService>(Lifetime.Singleton);

            composition.Register<UrlHelper>(Lifetime.Singleton);

            composition.Register<CurrencyHelper>(Lifetime.Singleton);

            composition.Register<MappingFieldHelper>(Lifetime.Singleton);

            composition.Register<IMappingService<Mapping>, MappingService>(Lifetime.Singleton);
        }
#endif
    }
}
