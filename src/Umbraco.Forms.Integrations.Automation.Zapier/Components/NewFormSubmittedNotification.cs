#if NETCOREAPP
using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common;
using Umbraco.Extensions;
using Umbraco.Forms.Core.Services;
using Umbraco.Forms.Core.Services.Notifications;
using Umbraco.Forms.Integrations.Automation.Zapier.Extensions;
using Umbraco.Forms.Integrations.Automation.Zapier.Helpers;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Components
{
    public class NewFormSubmittedNotification : INotificationHandler<RecordCreatingNotification>
    {
        private readonly IUmbracoHelperAccessor _umbracoHelperAccessor;

        private readonly IPublishedUrlProvider _publishedUrlProvider;

        private readonly IFormService _formService;

        private readonly ZapierService _zapierService;

        private readonly ZapierFormSubscriptionHookService _zapierFormSubscriptionHookService;

        private readonly ILogger<NewFormSubmittedNotification> _logger;

        public NewFormSubmittedNotification(
            IUmbracoHelperAccessor umbracoHelperAccessor,
            IPublishedUrlProvider publishedUrlProvider,
            IFormService formService, 
            ZapierService zapierService, ZapierFormSubscriptionHookService zapierFormSubscriptionHookService,
            ILogger<NewFormSubmittedNotification> logger)
        {
            _umbracoHelperAccessor = umbracoHelperAccessor;

            _publishedUrlProvider = publishedUrlProvider;

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

                if (_zapierFormSubscriptionHookService.TryGetByName(form.Name, out var zapFormConfigList))
                {
                    string pageUrl = string.Empty;
                    if (_umbracoHelperAccessor.TryGetUmbracoHelper(out UmbracoHelper umbracoHelper))
                    {
                        IPublishedContent publishedContent = umbracoHelper.Content(notificationSavedEntity.UmbracoPageId);
                        if (publishedContent != null)
                        {
                            pageUrl = publishedContent.Url(_publishedUrlProvider, mode: UrlMode.Absolute);
                        }
                    }

                    var content = form.ToFormDictionary(notificationSavedEntity, pageUrl);
                    
                    foreach (var formConfigDto in zapFormConfigList)
                    {
                        var result =
                            triggerHelper.FormExecute(formConfigDto.HookUrl, content);

                        if(!string.IsNullOrEmpty(result))
                            _logger.LogError(result);
                    }
                }
            }
        }
    }
}
#endif