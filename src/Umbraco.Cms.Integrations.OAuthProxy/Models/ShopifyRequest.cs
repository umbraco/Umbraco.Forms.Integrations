using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Umbraco.Cms.Integrations.OAuthProxy.Models
{
    public class ShopifyRequest
    {
        private readonly IQueryCollection _collection;

        public ShopifyRequest(IQueryCollection collection) => _collection = collection;

        public string Shop => _collection["shop"];

        public string Hmac => _collection["hmac"];

        public string Host => _collection["host"];

        public string Timestamp => _collection["timestamp"];

        public bool IsInstallationRequestFlow => !string.IsNullOrEmpty(Shop) && !string.IsNullOrEmpty(Hmac);

        public string GetHmacBody()
        {
            var dict = _collection
                .ToDictionary(x => x.Key, x => x.Value)
                .Where(p => p.Key != "hmac")
                .OrderBy(p => p.Key);
            var sb = new StringBuilder();
            foreach (var kvp in dict)
            {
                if (dict.First().Key != kvp.Key)
                {
                    sb.Append("&");
                }
                sb.Append($"{kvp.Key}={kvp.Value}");
            }

            return sb.ToString();
        }

        public string GetHmacValue(string secretKey, string body)
        {
            using HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));

            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        public bool IsShopValid() => Shop.Contains("myshopify.com");

    }
}
