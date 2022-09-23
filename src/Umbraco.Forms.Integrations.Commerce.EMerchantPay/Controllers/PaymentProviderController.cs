using Microsoft.AspNetCore.Mvc;

using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System;

using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Forms.Core.Data.Storage;
using Umbraco.Forms.Core.Services;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Services;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Controllers
{
    public class PaymentProviderController : UmbracoApiController
    {
        private readonly PaymentService _paymentService;

        private readonly IRecordStorage _recordStorage;

        private readonly IRecordService _recordService;

        private readonly IFormService _formService;
        
        public PaymentProviderController(PaymentService paymentService, IRecordStorage recordStorage, IRecordService recordService, IFormService formService)
        {
            _paymentService = paymentService;

            _recordStorage = recordStorage;

            _recordService = recordService;

            _formService = formService;
        }

        [HttpPost]
        public HttpResponseMessage NotifyPayment(string formId, string recordUniqueId, string statusFieldId, bool approve, [FromForm] NotificationDto notificationDto)
        {
            try
            {
                // reconcile
                var reconcileTask =
                    Task.Run(async () => await _paymentService.Reconcile(notificationDto.UniqueId));

                var reconcileResponse = reconcileTask.Result;

                // get record with uniqueId and update status
                var form = _formService.Get(Guid.Parse(formId));

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