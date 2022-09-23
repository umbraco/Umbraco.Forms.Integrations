using System;
using System.Net.Http;
using System.Threading.Tasks;

using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Configuration;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos;

using Microsoft.Extensions.Options;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Services
{
    public class ConsumerService : BaseService<ConsumerDto>
    {
        private readonly PaymentProviderSettings Options;

        private readonly IHttpClientFactory _httpClientFactory;

        public ConsumerService(IOptions<PaymentProviderSettings> options, IHttpClientFactory httpClientFactory)
        {
            Options = options.Value;

            _httpClientFactory = httpClientFactory;
        }

        public async Task<ConsumerDto> Create(ConsumerDto consumer)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.HttpClients.GatewayClient);

            var consumerRequestContent = Serialize(consumer, Constants.RootNode.CreateConsumerRequest);

            var consumerResponse = await httpClient
                .PostAsync("v1/create_consumer", consumerRequestContent);

            var response = await consumerResponse.Content.ReadAsStringAsync();

            return Deserialize(response, Constants.RootNode.CreateConsumerResponse);
        }

        public async Task<ConsumerDto> Retrieve(ConsumerDto consumer)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.HttpClients.GatewayClient);

            var consumerRequestContent = Serialize(consumer, Constants.RootNode.RetrieveConsumerRequest);

            var consumerResponse = await httpClient
                .PostAsync("v1/retrieve_consumer", consumerRequestContent);

            var response = await consumerResponse.Content.ReadAsStringAsync();

            return Deserialize(response, Constants.RootNode.RetrieveConsumerResponse);
        }


    }
}