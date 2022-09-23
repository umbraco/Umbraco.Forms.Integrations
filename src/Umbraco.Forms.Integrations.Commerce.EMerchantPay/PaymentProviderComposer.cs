using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Net.Http.Headers;
using System.Text;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Forms.Core.Providers;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Configuration;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Helpers;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Services;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay
{
    public class PaymentProviderComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var options = builder.Services
               .AddOptions<PaymentProviderSettings>()
               .Bind(builder.Config.GetSection(Constants.Configuration.Settings));

            builder.WithCollectionBuilder<WorkflowCollectionBuilder>()
                .Add<PaymentProviderWorkflow>();

            var byteArray = Encoding.ASCII.GetBytes(
                $"{builder.Config.GetSection(Constants.Configuration.Settings)[nameof(PaymentProviderSettings.Username)]}" +
                $":{builder.Config.GetSection(Constants.Configuration.Settings)[nameof(PaymentProviderSettings.Password)]}");

            builder.Services.AddHttpClient(Constants.HttpClients.GatewayClient, config =>
            {
                config.BaseAddress = new Uri(builder.Config.GetSection(Constants.Configuration.Settings)[nameof(PaymentProviderSettings.GatewayBaseUrl)]);
                config.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            });

            builder.Services.AddHttpClient(Constants.HttpClients.WpfClient, config =>
            {
                config.BaseAddress = new Uri(builder.Config.GetSection(Constants.Configuration.Settings)[nameof(PaymentProviderSettings.WpfUrl)]);
                config.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            });

            builder.Services.AddSingleton<ConsumerService>();

            builder.Services.AddSingleton<PaymentService>();

            builder.Services.AddSingleton<UrlHelper>();

            builder.Services.AddSingleton<CurrencyHelper>();

            builder.Services.AddSingleton<MappingFieldHelper>();

            builder.Services.AddSingleton<IMappingService<Mapping>, MappingService>();

            builder.Services.AddSingleton<ISettingsParser, SettingsParser>();
        }
    }
}
