using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Integrations.Authorization.Core.Interfaces;

namespace Umbraco.Cms.Integrations.Authorization.Core
{
    public static class Dependencies
    {
        public static void AddAuthorizationModule(this IServiceCollection services)
        {
            services.AddHttpClient();
        }
    }
}
