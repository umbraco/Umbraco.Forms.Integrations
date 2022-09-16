using System;
using System.Collections.Generic;

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
            { "Dynamics", "oauth2/v2.0/token" }
        };

        public static void AddServiceClients(this IServiceCollection services)
        {
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
                c.BaseAddress = new Uri("https://shop-replace.myshopify.com/admin/");
            });
            services.AddHttpClient("GoogleToken", c =>
            {
                c.BaseAddress = new Uri("https://oauth2.googleapis.com/");
            });
            services.AddHttpClient("DynamicsToken", c =>
            {
                c.BaseAddress = new Uri("https://login.microsoftonline.com/common/");
            });
        }
    }
}
