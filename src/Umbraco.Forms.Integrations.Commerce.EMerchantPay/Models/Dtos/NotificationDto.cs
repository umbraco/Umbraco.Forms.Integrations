
namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Models.Dtos
{
    public class NotificationDto
    {
        /// <summary>
        /// Alias for TransactionId
        /// </summary>
        public string wpf_transaction_id { get; set; }

        public string TransactionId => wpf_transaction_id;

        /// <summary>
        /// Alias for Unique ID
        /// </summary>
        public string wpf_unique_id { get; set; }

        public string UniqueId => wpf_unique_id;

        public string wpf_status { get; set; }

        /// <summary>
        /// Alias for Status
        /// </summary>
        public string Status => wpf_status;
    }
}
