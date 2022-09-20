
namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay
{
    public class Constants
    {
        public const string WorkflowId = "b2731255-2e48-4345-9ae5-aaf5b8bfb10a";

        public static class Configuration
        {
            public const string Settings = "Umbraco:Forms:Integrations:Commerce:eMerchantPay:Settings";

            public const string GatewayBaseUrlKey = "Umbraco.Forms.Integrations.Commerce.eMerchantPay.GatewayBaseurl";

            public const string WpfUrlKey = "Umbraco.Forms.Integrations.Commerce.eMerchantPay.WpfUrl";

            public const string UsernameKey = "Umbraco.Forms.Integrations.Commerce.eMerchantPay.Username";

            public const string PasswordKey = "Umbraco.Forms.Integrations.Commerce.eMerchantPay.Password";

            public const string SupplierKey = "Umbraco.Forms.Integrations.Commerce.eMerchantPay.Supplier";

            public const string UsageKey = "Umbraco.Forms.Integrations.Commerce.eMerchantPay.Usage";

            public const string CurrenciesKey = "Umbraco.Forms.Integrations.Commerce.eMerchantPay.Currencies";

            public const string TransactionTypesKey = "Umbraco.Forms.Integrations.Commerce.eMerchantPay.TransactionTypes";

            public const string MappingFieldsKey = "Umbraco.Forms.Integrations.Commerce.eMerchantPay.MappingFields";

            public const string UmbracoBaseUrlKey = "Umbraco.Forms.Integrations.Commerce.eMerchantPay.UmbracoBaseUrl";
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
    }
}
