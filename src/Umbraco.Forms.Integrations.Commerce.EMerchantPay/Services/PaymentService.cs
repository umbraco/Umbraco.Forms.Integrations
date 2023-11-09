using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Configuration;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Services;

public class PaymentService : BaseService<PaymentDto>
{
    private readonly PaymentProviderSettings Options;

    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentService(IOptions<PaymentProviderSettings> options, IHttpClientFactory httpClientFactory)
    {
        Options = options.Value;

        _httpClientFactory = httpClientFactory;
    }

    public async Task<PaymentDto> Create(PaymentDto payment)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.HttpClients.WpfClient);

        var paymentRequestContent = Serialize(payment, Constants.RootNode.WpfPayment);

        var paymentResponse = await httpClient
            .PostAsync(string.Empty, paymentRequestContent);

        var response = await paymentResponse.Content.ReadAsStringAsync();

        return Deserialize(response, Constants.RootNode.WpfPayment);
    }

    public async Task<PaymentDto> Reconcile(string uniqueId)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.HttpClients.WpfClient);

        var reconcileRequestContent =
            Serialize(new PaymentDto { UniqueId = uniqueId }, Constants.RootNode.WpfReconcile);

        var reconcileResponse = await httpClient
            .PostAsync("reconcile", reconcileRequestContent);

        var response = await reconcileResponse.Content.ReadAsStringAsync();

        return Deserialize(response, Constants.RootNode.WpfPayment);
    }
}