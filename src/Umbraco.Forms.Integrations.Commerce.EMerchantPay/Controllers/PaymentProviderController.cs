#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;

using Umbraco.Cms.Web.Common.Controllers;
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
using Umbraco.Forms.Core.Services;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Controllers
{
    public class PaymentProviderController : UmbracoApiController
    {
        private readonly PaymentService _paymentService;

        private readonly IRecordStorage _recordStorage;

        private readonly IRecordService _recordService;

#if NETCOREAPP
        private readonly IFormService _formService;

        public PaymentProviderController(PaymentService paymentService, IRecordStorage recordStorage, IRecordService recordService, IFormService formService)
#else
        private readonly IFormStorage _formStorage;

        public PaymentProviderController(PaymentService paymentService, IRecordStorage recordStorage, IRecordService recordService, IFormStorage formStorage)
#endif
        {
            _paymentService = paymentService;

            _recordStorage = recordStorage;

            _recordService = recordService;

#if NETCOREAPP
            _formService = formService;
#else
            _formStorage = formStorage;
#endif
        }

        [HttpPost]
#if NETCOREAPP
        public HttpResponseMessage NotifyPayment(string formId, string recordUniqueId, string statusFieldId, bool approve, [FromForm] NotificationDto notificationDto)
#else
        public HttpResponseMessage NotifyPayment(string formId, string recordUniqueId, string statusFieldId, bool approve, [FromBody] NotificationDto notificationDto)
#endif
        {
            try
            {
                // reconcile
                var reconcileTask =
                    Task.Run(async () => await _paymentService.Reconcile(notificationDto.UniqueId));

                var reconcileResponse = reconcileTask.Result;

                // get record with uniqueId and update status
#if NETCOREAPP
            var form = _formService.Get(Guid.Parse(formId));
#else
                var form = _formStorage.GetForm(Guid.Parse(formId));
#endif

                var record = _recordStorage.GetRecordByUniqueId(Guid.Parse(recordUniqueId), form);

                var paymentStatusField = record.GetRecordField(Guid.Parse(statusFieldId));
                paymentStatusField.Values.Clear();
                paymentStatusField.Values.Add(reconcileResponse.Status);

                _recordStorage.UpdateRecord(record, form);

                if (approve && notificationDto.Status == Constants.PaymentStatus.Approved)
                {
                    _recordService.Approve(record, form);
                }

                if (reconcileResponse.Status == Constants.ErrorCode.WorkflowError)
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);

                string notificationXml = $"<notification_echo><wpf_unique_id>{notificationDto.UniqueId}</wpf_unique_id></notification_echo>";
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
