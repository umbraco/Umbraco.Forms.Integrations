
namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay
{
    public class Constants
    {
        public const string WorkflowId = "b2731255-2e48-4345-9ae5-aaf5b8bfb10a";

        public const int RequiredMappingsNo = 9;

        public static class Configuration
        {
            public const string Settings = "Umbraco:Forms:Integrations:Commerce:eMerchantPay:Settings";

            public const string GatewayBaseUrlKey = "Umbraco.Forms.Integrations.Commerce.eMerchantPay.GatewayBaseurl";

            public const string WpfUrlKey = "Umbraco.Forms.Integrations.Commerce.eMerchantPay.WpfUrl";

            public const string UsernameKey = "Umbraco.Forms.Integrations.Commerce.eMerchantPay.Username";

            public const string PasswordKey = "Umbraco.Forms.Integrations.Commerce.eMerchantPay.Password";
        }

        public static class ErrorCode
        {
            public const string ConsumerExists = "701";
        }

        public static class RootNode
        {
            public const string CreateConsumerRequest = "create_consumer_request";

            public const string CreateConsumerResponse = "create_consumer_response";

            public const string RetrieveConsumerRequest = "retrieve_consumer_request";

            public const string RetrieveConsumerResponse = "retrieve_consumer_response";

            public const string WpfPayment = "wpf_payment";
        }
    }
}
