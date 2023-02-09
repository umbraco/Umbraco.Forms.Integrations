using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Integrations.OAuthProxy.Configuration
{
    public static class ServiceConfiguration
    {
        /// <summary>
        /// Integrated services with their token URIs
        /// </summary>
        public static Dictionary<string, string> ServiceProviders = new()
        {
            { "Hubspot", "oauth/v1/token" }, 
            { "HubspotForms", "oauth/v1/token" }, 
            { "Semrush", "oauth2/access_token" }, 
            { "Shopify", "oauth/access_token" },
            { "Google", "token"},
            { "Dynamics", "oauth2/v2.0/token" },
            { "Aprimo", "login/connect/token" }
        };

        public static void AddServiceClients(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            var serviceProvider = services.BuildServiceProvider();

            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();

            services.AddHttpClient("HubspotToken", c =>
            {
                c.BaseAddress = new Uri("https://api.hubapi.com/");
            });
            services.AddHttpClient("HubspotFormsToken", c =>
            {
                c.BaseAddress = new Uri("https://api.hubapi.com/");
            });
            services.AddHttpClient("SemrushToken", c =>
            {
                c.BaseAddress = new Uri("https://oauth.semrush.com/");
            });
            services.AddHttpClient("ShopifyToken", c =>
            {
                const string prefix = "service_address_";

                var serviceAddressReplaceHeader = httpContextAccessor.HttpContext.Request.Headers
                    .First(p => p.Key.Contains(prefix));

                c.BaseAddress = new Uri($"https://{serviceAddressReplaceHeader.Value}.myshopify.com/admin/");
            });
            services.AddHttpClient("GoogleToken", c =>
            {
                c.BaseAddress = new Uri("https://oauth2.googleapis.com/");
            });
            services.AddHttpClient("DynamicsToken", c =>
            {
                c.BaseAddress = new Uri("https://login.microsoftonline.com/common/");
            });
            services.AddHttpClient("AprimoToken", c =>
            {
                c.BaseAddress = new Uri($"https://{httpContextAccessor.HttpContext.Request.Headers["tenant"].First()}.aprimo.com/");
            });
        }
    }
}
