#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Linq;

using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Forms.Core.Data.Storage;
using Umbraco.Forms.Integrations.Automation.Zapier.Helpers;
using Umbraco.Forms.Integrations.Automation.Zapier.Models.Dtos;
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

            if (_zapierFormSubscriptionHookService.TryGetByName(e.Form.Name, out var zapFormConfigList))
            {
                var content = new Dictionary<string, string>
                {
                    { Constants.Form.Id, e.Form.Id.ToString() },
                    { Constants.Form.Name, e.Form.Name },
                    { Constants.Form.SubmissionDate, DateTime.UtcNow.ToString("s") },
                    { Constants.Form.PageUrl, pageUrl }
                };

                foreach (var recordField in e.Record.RecordFields)
                {
                    content.Add(recordField.Value.Alias, recordField.Value.ValuesAsString());
                }

                foreach (var zapFormConfig in zapFormConfigList)
                {
                    var result = triggerHelper.FormExecute(zapFormConfig.HookUrl, content);

                    if (!string.IsNullOrEmpty(result))
                        _logger.Error<NewFormSubmittedComponent>(result);
                }
            }
        }
    }
}
#endif
