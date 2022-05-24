#if NETFRAMEWORK
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Forms.Core.Data.Storage;
using Umbraco.Forms.Integrations.Automation.Zapier.Extensions;
using Umbraco.Forms.Integrations.Automation.Zapier.Helpers;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;
using Umbraco.Web;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class NewFormSubmittedComposer : ComponentComposer<NewFormSubmittedComponent>
    { }

    public class NewFormSubmittedComponent : IComponent
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        private readonly IRecordStorage _recordStorage;

        private readonly ZapierService _zapierService;

        private readonly ZapierFormSubscriptionHookService _zapierFormSubscriptionHookService;
        
        private readonly ILogger _logger;

        public NewFormSubmittedComponent(IUmbracoContextAccessor umbracoContextAccessor, IRecordStorage recordStorage, 
            ZapierService zapierService, 
            ZapierFormSubscriptionHookService zapierFormSubscriptionHookService,
            ILogger logger)
        {
            _umbracoContextAccessor = umbracoContextAccessor;

            _recordStorage = recordStorage;

            _zapierService = zapierService;

            _zapierFormSubscriptionHookService = zapierFormSubscriptionHookService;

            _logger = logger;
        }

        public void Initialize()
        {
            _recordStorage.RecordInserting += RecordStorage_RecordInserting;
        }

        public void Terminate()
        {
        }

        private void RecordStorage_RecordInserting(object sender, Core.RecordEventArgs e)
        {
            var triggerHelper = new TriggerHelper(_zapierService);

            UmbracoContext umbracoContext = _umbracoContextAccessor.UmbracoContext;
            var umbracoPageId = e.Record.UmbracoPageId;
            var pageUrl = umbracoContext.UrlProvider.GetUrl(umbracoPageId, UrlMode.Absolute);

            if (_zapierFormSubscriptionHookService.TryGetById(e.Form.Id.ToString(), out var zapHookUrls))
            {
                var content = e.Form.ToFormDictionary(e.Record, pageUrl);

                foreach (var hookUrl in zapHookUrls)
                {
                    var result = triggerHelper.FormExecute(hookUrl, content);

                    if (!string.IsNullOrEmpty(result))
                        _logger.Error<NewFormSubmittedComponent>(result);
                }
            }
        }
    }
}
#endif
