
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Configuration;

#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Http.ModelBinding;
#endif

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Models.Dtos
{
    [ModelBinder(typeof(NotificationModelBinder))]
    public class NotificationDto
    {
        public string TransactionId { get; set; }

        public string UniqueId { get; set; }

        public string Status { get; set; }
    }
}
