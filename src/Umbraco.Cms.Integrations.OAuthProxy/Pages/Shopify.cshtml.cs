using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Umbraco.Cms.Integrations.OAuthProxy.Configuration;
using Umbraco.Cms.Integrations.OAuthProxy.Models;

namespace Umbraco.Cms.Integrations.OAuthProxy.Pages
{
    public class ShopifyModel : PageModel
    {
        private readonly AppSettings _appSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public string AuthorizationCode { get; set; }

        public ShopifyModel(IOptions<AppSettings> options, IHttpClientFactory httpClientFactory)
        {
            _appSettings = options.Value;
            _httpClientFactory = httpClientFactory;
        }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var shopifyRequest = new ShopifyRequest(Request.Query);
            // check if this is an installation request flow.
            if (shopifyRequest.IsInstallationRequestFlow)
            {
                var hmacBody = shopifyRequest.GetHmacBody();
                var hmacValue = shopifyRequest.GetHmacValue(_appSettings.ShopifyClientSecret, hmacBody);

                // verify shop name and HMAC string.
                if (!shopifyRequest.IsShopValid()
                    || !shopifyRequest.Hmac.Equals(hmacValue))
                {
                    context.Result = new BadRequestObjectResult("Installation verification failed.");
                    return;
                }

                var url = string.Format("https://{0}/admin/oauth/authorize?client_id={1}&scope={2}&redirect_uri={3}",
                        shopifyRequest.Shop,
                        _appSettings.ShopifyClientId,
                        "read_products",
                        $"{Request.Host.Value}/oauth/shopify");
                context.Result = new RedirectResult(url, true);
                return;
            }

            await next();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var shopifyRequest = new ShopifyRequest(Request.Query);
            if (shopifyRequest.IsInstallationRequestFlowWithCode)
            {
                var hmacBody = shopifyRequest.GetHmacBody();
                var hmacValue = shopifyRequest.GetHmacValue(_appSettings.ShopifyClientSecret, hmacBody);

                // verify shop name and HMAC string.
                if (!shopifyRequest.IsShopValid()
                    || !shopifyRequest.Hmac.Equals(hmacValue))
                {
                    return BadRequest("Installation verification failed.");
                }

                // retrieve access token
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri($"https://{shopifyRequest.Shop}");

                var requestUri = string.Format("/admin/oauth/access_token?client_id={0}&client_secret={1}&code={2}",
                    _appSettings.ShopifyClientId,
                    _appSettings.ShopifyClientSecret,
                    shopifyRequest.Code);
                var response = await client.PostAsync(requestUri, new StringContent(string.Empty));

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Could not retrieve the access token.");
                }

                return Redirect($"https://{shopifyRequest.Shop}");
            }

            AuthorizationCode = Request.Query["code"];

            return Page();
        }
    }
}
