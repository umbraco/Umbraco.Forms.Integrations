using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Models.Dtos
{
    public class NotificationDto
    {
        public string TransactionId { get; set; }

        public string wpf_unique_id { get; set; }

        public string Status { get; set; }
    }
}
