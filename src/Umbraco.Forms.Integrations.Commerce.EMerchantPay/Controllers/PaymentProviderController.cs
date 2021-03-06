#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;

using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Forms.Core.Services;
#else
using System.Web.Http;

using Umbraco.Web.WebApi;
#endif

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Umbraco.Forms.Core.Data.Storage;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Models.Dtos;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Services;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Controllers
{
    public class PaymentProviderController : UmbracoApiController
    {
        private readonly PaymentService _paymentService;

        private readonly IRecordStorage _recordStorage;

#if NETCOREAPP
        private readonly IFormService _formService;

        public PaymentProviderController(PaymentService paymentService, IRecordStorage recordStorage, IFormService formService)
#else
        private readonly IFormStorage _formStorage;

        public PaymentProviderController(PaymentService paymentService, IRecordStorage recordStorage, IFormStorage formStorage)
#endif
        {
            _paymentService = paymentService;

            _recordStorage = recordStorage;

#if NETCOREAPP
            _formService = formService;
#else
            _formStorage = formStorage;
#endif
        }

        [HttpPost]
        public HttpResponseMessage NotifyPayment(string formId, string recordUniqueId, string statusFieldId, [FromBody] NotificationDto notificationDto)
        {
            try
            {
                // reconcile
                var reconcileTask =
                    Task.Run(async () => await _paymentService.Reconcile(notificationDto.wpf_unique_id));

                var reconcileResponse = reconcileTask.Result;

                // get record with uniqueId and update status
#if NETCOREAPP
            var form = _formService.Get(Guid.Parse(formId));
#else
                var form = _formStorage.GetForm(Guid.Parse(formId));
#endif

                var record = _recordStorage.GetRecordByUniqueId(Guid.Parse(recordUniqueId), form);
                var paymentStatusField = record.GetRecordField(Guid.Parse(statusFieldId));
                paymentStatusField.Values.Add(reconcileResponse.Status);

                _recordStorage.UpdateRecord(record, form);

                if (reconcileResponse.Status == Constants.ErrorCode.WorkflowError)
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);

                string notificationXml = $"<notification_echo><wpf_unique_id>{notificationDto.wpf_unique_id}</wpf_unique_id></notification_echo>";
                return new HttpResponseMessage()
                {
                    Content = new StringContent(notificationXml, Encoding.UTF8, "application/xml")
                };
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

        }

    }
}
