using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

#if NETCOREAPP
using Microsoft.Extensions.Options;
#else
#endif

using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Configuration;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Models.Dtos;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Services
{
    public class PaymentService : BaseService<PaymentDto>
    {
        private readonly PaymentProviderSettings Options;

        // Using a static HttpClient (see: https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/).
        private readonly static HttpClient s_client = new HttpClient();

        // Access to the client within the class is via ClientFactory(), allowing us to mock the responses in tests.
        public static Func<HttpClient> ClientFactory = () => s_client;

#if NETCOREAPP
        public PaymentService(IOptions<PaymentProviderSettings> options)
#else
        public PaymentService()
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

        public async Task<PaymentDto> Create(PaymentDto payment)
        {
            var paymentRequestContent = Serialize(payment, Constants.RootNode.WpfPayment);

            var paymentResponse = await ClientFactory()
                .PostAsync(new Uri(Options.WpfUrl), paymentRequestContent);

            var response = await paymentResponse.Content.ReadAsStringAsync();

            return Deserialize(response, Constants.RootNode.WpfPayment);
        }

        public async Task<PaymentDto> Reconcile(string uniqueId)
        {
            var reconcileRequestContent =
                Serialize(new PaymentDto {UniqueId = uniqueId}, Constants.RootNode.WpfReconcile);

            var reconcileResponse = await ClientFactory()
                .PostAsync(new Uri($"{Options.WpfUrl}/reconcile"), reconcileRequestContent);

            var response = await reconcileResponse.Content.ReadAsStringAsync();

            return Deserialize(response, Constants.RootNode.WpfPayment);
        }
    }
}
