using System.Collections.Generic;
using System.Linq;

#if NETCOREAPP
using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Scoping;
#else
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
#endif

namespace Umbraco.Forms.Integrations.Automation.Zapier.Services
{
    public class ZapierFormSubscriptionHookService
    {
        private readonly IScopeProvider _scopeProvider;

#if NETCOREAPP
        private readonly ILogger<ZapierFormSubscriptionHookService> _logger;

        public ZapierFormSubscriptionHookService(IScopeProvider scopeProvider, ILogger<ZapierFormSubscriptionHookService> logger)
        {
            _scopeProvider = scopeProvider;

            _logger = logger;
        }
#else
        private readonly ILogger _logger;

         public ZapierFormSubscriptionHookService(IScopeProvider scopeProvider, ILogger logger)
        {
            _scopeProvider = scopeProvider;

            _logger = logger;
        }
#endif

        public bool TryGetById(string id, out IEnumerable<string> dto)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                var entities =
                    scope.Database
                        .Query<string>( "SELECT HookUrl FROM zapierSubscriptionHook where EntityId = @0 and Type = 2", id).ToArray();

                dto = entities;

                return entities.Any();
            }
        }
    }
}
