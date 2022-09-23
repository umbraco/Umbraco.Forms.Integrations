using Microsoft.AspNetCore.Mvc.ModelBinding;

using System;
using System.Threading.Tasks;

using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Configuration
{
    public sealed class NotificationModelBinder : IModelBinder
    {
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
    }
}