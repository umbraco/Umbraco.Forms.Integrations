using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Integrations.OAuthProxy.Configuration;

namespace Umbraco.Cms.Integrations.OAuthProxy.Filters
{
	public class SignatureValidationAttribute : ActionFilterAttribute
	{
		private const string HeaderKey = "X-Shopify-Hmac-Sha256";

		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var appSettings = context.HttpContext.RequestServices.GetService<IOptions<AppSettings>>().Value;

			var hmacHeaderValues = context.HttpContext.Request.Headers
				.FirstOrDefault(kvp => kvp.Key.Equals(HeaderKey, StringComparison.OrdinalIgnoreCase)).Value;

			if (hmacHeaderValues.Count == 0)
			{
				context.Result = new UnauthorizedResult();
				return;
			}

			string hmacHeader = hmacHeaderValues.First();

			HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSettings.ShopifyClientSecret));

			var bodyStream = new StreamReader(context.HttpContext.Request.Body);
			string requestBody = await bodyStream.ReadToEndAsync();
			string hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(requestBody)));

			if (hash != hmacHeader)
			{
				context.Result = new UnauthorizedResult();
				return;
			}
		}
	}
}
