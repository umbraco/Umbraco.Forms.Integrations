using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Builders;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.ExtensionMethods;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Models.Dtos;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Services;

#if NETCOREAPP
using Microsoft.AspNetCore.Http;

using Umbraco.Cms.Core.Web;
#else
using Umbraco.Web;
using Umbraco.Forms.Core.Persistence.Dtos;
#endif

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay
{
    public class PaymentProviderWorkflow : WorkflowType
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        private readonly ConsumerService _consumerService;

        private readonly PaymentService _paymentService;

        #region WorkflowSettings

        [Setting("Usage",
            Description = "Payment usage description",
            View = "TextField")]
        public string Usage { get; set; }

        [Setting("Amount",
            Description = "Payment amount (without decimals)",
            View = "TextField")]
        public string Amount { get; set; }

        [Setting("Currency",
            Description = "Payment currency",
            PreValues = "DKK,EUR,USD",
            View = "Dropdownlist")]
        public string Currency { get; set; }

        [Setting("Supplier",
            Description = "Name of business supplier",
            View = "TextField")]
        public string Supplier { get; set; }

        [Setting("CustomerDetails",
            Description = "Map customer details with form fields",
            View = "~/App_Plugins/UmbracoForms.Integrations/Commerce/eMerchantPay/customer-details-mapper.html")]
        public string CustomerDetailsMappings { get; set; }

        [Setting("Success URL",
            View = "Pickers.Content")]
        public string SuccessUrl { get; set; }

        [Setting("Failure URL",
            View = "Pickers.Content")]
        public string FailureUrl { get; set; }

        [Setting("Cancel URL",
            View = "Pickers.Content")]
        public string CancelUrl { get; set; }

        [Setting("Notification URL",
            View = "Pickers.Content")]
        public string NotificationUrl { get; set; }

        #endregion

#if NETCOREAPP
        public PaymentProviderWorkflow(IHttpContextAccessor httpContextAccessor,
            ConsumerService consumerService, PaymentService paymentService)
#else
        public PaymentProviderWorkflow(IHttpContextAccessor httpContextAccessor, IUmbracoContextAccessor umbracoContextAccessor, 
                ConsumerService consumerService, PaymentService paymentService)
#endif
        {
            Id = new Guid(Constants.WorkflowId);
            Name = "eMerchantPay Gateway";
            Description = "eMerchantPay provider handling form-based payments.";
            Icon = "icon-multiple-credit-cards";

            _consumerService = consumerService;

            _paymentService = paymentService;

            _httpContextAccessor = httpContextAccessor;

#if NETCOREAPP
#else
            _umbracoContextAccessor = umbracoContextAccessor;
#endif
        }

#if NETCOREAPP
        public override WorkflowExecutionStatus Execute(WorkflowExecutionContext context)
#else
        public override WorkflowExecutionStatus Execute(Record record, RecordEventArgs e)
#endif
        {
            if (!CustomerDetailsMappings.TryParseMappings(out var mappings)) return WorkflowExecutionStatus.Failed;

            var mappingBuilder = new MappingBuilder()
#if NETCOREAPP
                .SetValues(context.Record, mappings)
#else
                .SetValues(record, mappings)
#endif
                .Build();

            // step 1. Create or Retrieve Consumer
            var consumer = new ConsumerDto { Email = mappingBuilder.Email };

            // step 1. Create Consumer
            var createConsumerTask = Task.Run(async () => await _consumerService.Create(consumer));

            var result = createConsumerTask.Result;
            if (result.Code == Constants.ErrorCode.ConsumerExists)
            {
                // step 1.1. Get Consumer
                var retrieveConsumerTask = Task.Run(async () => await _consumerService.Retrieve(consumer));
                consumer = retrieveConsumerTask.Result;
            }

            // step 2. Create Payment
            var transactionId = Guid.NewGuid();

            var payment = new PaymentDto
            {
                TransactionId = transactionId.ToString(),
                Usage = Usage,
                NotificationUrl = "https://www.example.com/notification",
                ReturnSuccessUrl = "https://www.example.com/success",
                ReturnFailureUrl = "https://www.example.com/failure",
                ReturnCancelUrl = "https://www.example.com/cancel",
                Amount = int.Parse(Amount),
                Currency = Currency,
                ConsumerId = consumer.Id,
                CustomerEmail = consumer.Email,
                CustomerPhone = mappingBuilder.Phone,
                BillingAddress = new AddressDto
                {
                    FirstName = mappingBuilder.FirstName,
                    LastName = mappingBuilder.LastName,
                    Address1 = mappingBuilder.Address,
                    Address2 = string.Empty,
                    ZipCode = mappingBuilder.ZipCode,
                    City = mappingBuilder.City,
                    State = mappingBuilder.State,
                    Country = mappingBuilder.Country
                },
                BusinessAttribute = new BusinessAttribute { NameOfTheSupplier = Supplier },
                TransactionTypes = new TransactionTypeDto
                {
                    TransactionTypes = new List<TransactionTypeRecordDto>
                    {
                        new TransactionTypeRecordDto { TransactionType = "authorize" },
                        new TransactionTypeRecordDto { TransactionType = "sale" }
                    }
                }
            };

            var createPaymentTask = Task.Run(async () => await _paymentService.Create(payment));

            var createPaymentResult = createPaymentTask.Result;

            if (createPaymentResult.Status != "error")
            {
                // TODO - update after Forms patch applied
                _httpContextAccessor.HttpContext.Items["RedirectAfterFormSubmitUrl"] = createPaymentResult.RedirectUrl;

                return WorkflowExecutionStatus.Completed;
            }
            else
                return WorkflowExecutionStatus.Failed;
        }

        public override List<Exception> ValidateSettings()
        {
            var list = new List<Exception>();

            if(string.IsNullOrEmpty(Usage)) list.Add(new Exception("Usage value is not valid."));

            if(string.IsNullOrEmpty(Amount) || !int.TryParse(Amount, out _)) list.Add(new Exception("Amount value is not valid."));

            if(string.IsNullOrEmpty(Currency)) list.Add(new Exception("Currency field is required."));

            if(!CustomerDetailsMappings.TryParseMappings(out _)) list.Add(new Exception("Customer details mappings are required."));

            if (!SuccessUrl.IsContentValid(nameof(SuccessUrl), out var successError)) list.Add(new Exception(successError));

            if (!FailureUrl.IsContentValid(nameof(FailureUrl), out var failureError)) list.Add(new Exception(failureError));

            if (!CancelUrl.IsContentValid(nameof(CancelUrl), out var cancelError)) list.Add(new Exception(cancelError));

            if (!NotificationUrl.IsContentValid(nameof(NotificationUrl), out var notificationError)) list.Add(new Exception(notificationError));

            return list;
        }
    }
}
