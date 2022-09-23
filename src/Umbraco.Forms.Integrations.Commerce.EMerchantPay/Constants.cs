
namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay
{
    public class Constants
    {
        public const string WorkflowId = "b2731255-2e48-4345-9ae5-aaf5b8bfb10a";

        public static class Configuration
        {
            public const string Settings = "Umbraco:Forms:Integrations:Commerce:Emerchantpay:Settings";
        }

        public static class ErrorCode
        {
            public const string ConsumerExists = "701";

            public const string WorkflowError = "400";
        }

        public static class RootNode
        {
            public const string CreateConsumerRequest = "create_consumer_request";

            public const string CreateConsumerResponse = "create_consumer_response";

            public const string RetrieveConsumerRequest = "retrieve_consumer_request";

            public const string RetrieveConsumerResponse = "retrieve_consumer_response";

            public const string WpfPayment = "wpf_payment";

            public const string WpfReconcile = "wpf_reconcile";
        }

        public static class NotificationProperty
        {
            public const string TransactionId = "wpf_transaction_id";

            public const string UniqueId = "wpf_unique_id";

            public const string Status = "wpf_status";
        }

        public static class PaymentStatus
        {
            public const string Approved = "approved";
        }

        public static class HttpClients
        {
            public const string GatewayClient = "GatewayClient";

            public const string WpfClient = "WpfClient";
        }
    }
}
