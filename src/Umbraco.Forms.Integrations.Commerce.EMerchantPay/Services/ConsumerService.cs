using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Configuration;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos;

#if NETCOREAPP
using Microsoft.Extensions.Options;
#else
using System.Configuration;
#endif

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Services
{
    public class ConsumerService : BaseService<ConsumerDto>
    {
        private readonly PaymentProviderSettings Options;

        // Using a static HttpClient (see: https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/).
        private readonly static HttpClient s_client = new HttpClient();

        // Access to the client within the class is via ClientFactory(), allowing us to mock the responses in tests.
        public static Func<HttpClient> ClientFactory = () => s_client;

#if NETCOREAPP
        public ConsumerService(IOptions<PaymentProviderSettings> options)
#else
        public ConsumerService()
#endif
        {
#if NETCOREAPP
            Options = options.Value;
#else
            Options = new PaymentProviderSettings(ConfigurationManager.AppSettings);
#endif
            var byteArray = Encoding.ASCII.GetBytes($"{Options.Username}:{Options.Password}");

            s_client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public async Task<ConsumerDto> Create(ConsumerDto consumer)
        {
            var consumerRequestContent = Serialize(consumer, Constants.RootNode.CreateConsumerRequest);

            var consumerResponse = await ClientFactory()
                .PostAsync(new Uri($"{Options.GatewayBaseUrl}/v1/create_consumer"), consumerRequestContent);

            var response = await consumerResponse.Content.ReadAsStringAsync();

            return Deserialize(response, Constants.RootNode.CreateConsumerResponse);
        }

        public async Task<ConsumerDto> Retrieve(ConsumerDto consumer)
        {
            var consumerRequestContent = Serialize(consumer, Constants.RootNode.RetrieveConsumerRequest);

            var consumerResponse = await ClientFactory()
                .PostAsync(new Uri($"{Options.GatewayBaseUrl}/v1/retrieve_consumer"), consumerRequestContent);

            var response = await consumerResponse.Content.ReadAsStringAsync();

            return Deserialize(response, Constants.RootNode.RetrieveConsumerResponse);
        }

        
    }
}
