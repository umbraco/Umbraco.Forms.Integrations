#if NETCOREAPP
using Microsoft.AspNetCore.Mvc.ModelBinding;
#else
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
#endif

using System;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Models.Dtos;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Configuration
{
    public sealed class NotificationModelBinder : IModelBinder
    {
#if NETCOREAPP
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var transactionId = bindingContext.ValueProvider.GetValue(Constants.NotificationProperty.TransactionId);

            var uniqueId = bindingContext.ValueProvider.GetValue(Constants.NotificationProperty.UniqueId);

            var status = bindingContext.ValueProvider.GetValue(Constants.NotificationProperty.Status);

            var model = new NotificationDto
            {
                TransactionId = transactionId.FirstValue.ToString(),
                UniqueId = uniqueId.FirstValue.ToString(),
                Status = status.FirstValue.ToString()
            };

            bindingContext.Result = ModelBindingResult.Success(model);

            return Task.CompletedTask;
        }
#else
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var notification = new NotificationDto();

            var transactionIdValueResult = bindingContext.ValueProvider
                .GetValue(CreateFullPropertyName(bindingContext, Constants.NotificationProperty.TransactionId));
            if (transactionIdValueResult != null)
            {
                notification.TransactionId = transactionIdValueResult.AttemptedValue;
            }

            var uniqueIdValueResult = bindingContext.ValueProvider.GetValue(CreateFullPropertyName(bindingContext, Constants.NotificationProperty.UniqueId));
            if (uniqueIdValueResult != null)
                notification.UniqueId= uniqueIdValueResult.AttemptedValue;

            var statusValueResult = bindingContext.ValueProvider.GetValue(CreateFullPropertyName(bindingContext, Constants.NotificationProperty.Status));
            if (statusValueResult != null)
            {
                notification.Status = statusValueResult.AttemptedValue;
            }

            bindingContext.Model = notification;

            bindingContext.ValidationNode.ValidateAllProperties = true;

            return true;
        }
#endif

        private string CreateFullPropertyName(ModelBindingContext bindingContext, string propertyName)
        {
            if (string.IsNullOrEmpty(bindingContext.ModelName))
            {
                return propertyName;
            }
            return bindingContext.ModelName + "." + propertyName;
        }

    }
}
