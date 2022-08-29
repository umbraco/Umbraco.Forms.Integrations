using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Forms.Core.Services;
using Umbraco.Forms.Core.Services.Notifications;
using Umbraco.Forms.Integrations.Automation.Zapier.Extensions;
using Umbraco.Forms.Integrations.Automation.Zapier.Helpers;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Components
{
    public class NewFormSubmittedNotification : INotificationHandler<RecordCreatingNotification>
    {
        private readonly UmbUrlHelper _umbUrlHelper;

        private readonly IFormService _formService;

        private readonly ZapierService _zapierService;

        private readonly ZapierFormSubscriptionHookService _zapierFormSubscriptionHookService;

        private readonly ILogger<NewFormSubmittedNotification> _logger;

        public NewFormSubmittedNotification(
            UmbUrlHelper umbUrlHelper,
            IFormService formService, 
            ZapierService zapierService, ZapierFormSubscriptionHookService zapierFormSubscriptionHookService,
            ILogger<NewFormSubmittedNotification> logger)
        {
            _umbUrlHelper = umbUrlHelper;

            _formService = formService;

            _zapierService = zapierService;

            _zapierFormSubscriptionHookService = zapierFormSubscriptionHookService;

            _logger = logger;
        }

        public void Handle(RecordCreatingNotification notification)
        {
            var triggerHelper = new TriggerHelper(_zapierService);

            foreach (var notificationSavedEntity in notification.SavedEntities)
            {
                var form = _formService.Get(notificationSavedEntity.Form);

                if (_zapierFormSubscriptionHookService.TryGetById(form.Id.ToString(), out var subscriptionHooks))
                {
                    var content = form.ToFormDictionary(notificationSavedEntity, _umbUrlHelper.GetPageUrl(notificationSavedEntity.UmbracoPageId));

                    foreach (var subscriptionHook in subscriptionHooks)
                    {
                        var result =
                            triggerHelper.FormExecute(subscriptionHook.HookUrl, content);

                        if (!string.IsNullOrEmpty(result))
                            _logger.LogError(result);
                    }
                }
            }
        }
    }
}