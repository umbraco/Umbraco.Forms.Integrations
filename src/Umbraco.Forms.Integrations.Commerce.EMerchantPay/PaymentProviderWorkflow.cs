using System;
using System.Collections.Generic;

using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Models.Dtos;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Services;

#if NETCOREAPP
#else
using System.Threading.Tasks;
using Umbraco.Forms.Core.Persistence.Dtos;
#endif

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay
{
    public class PaymentProviderWorkflow : WorkflowType
    {
        private readonly ConsumerService _consumerService;

        private readonly PaymentService _paymentService;

        public PaymentProviderWorkflow(ConsumerService consumerService, PaymentService paymentService)
        {
            Id = new Guid(Constants.WorkflowId);
            Name = "eMerchantPay Gateway";
            Description = "eMerchantPay provider handling form-based payments.";
            Icon = "icon-multiple-credit-cards";

            _consumerService = consumerService;

            _paymentService = paymentService;
        }

#if NETCOREAPP
        public override WorkflowExecutionStatus Execute(WorkflowExecutionContext context)
        {
            return WorkflowExecutionStatus.Completed;
        }
#else
        public override WorkflowExecutionStatus Execute(Record record, RecordEventArgs e)
        {
            var consumer = new ConsumerDto {Email = "aco@umbraco.com"};

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
                Usage = "Umbraco Forms Test",
                NotificationUrl = "https://www.example.com/notification",
                ReturnSuccessUrl = "https://www.example.com/success",
                ReturnFailureUrl = "https://www.example.com/failure",
                ReturnCancelUrl = "https://www.example.com/cancel",
                Amount = 100,
                Currency = "EUR",
                ConsumerId = consumer.Id,
                CustomerEmail = consumer.Email,
                CustomerPhone = "+1987987987987",
                BillingAddress = new AddressDto
                {
                    FirstName = "Adrian",
                    LastName = "Cojocariu",
                    Address1 = "TEST1",
                    Address2 = "TEST2",
                    ZipCode = "10178",
                    City = "Los Angeles",
                    State = "CA",
                    Country = "US"
                },
                BusinessAttribute = new BusinessAttribute { NameOfTheSupplier = "Umbraco" },
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

            return WorkflowExecutionStatus.Completed;
        }
#endif

        public override List<Exception> ValidateSettings()
        {
            return new List<Exception>();
        }
    }
}
