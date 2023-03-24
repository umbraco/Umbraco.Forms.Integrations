using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Umbraco.Cms.Integrations.OAuthProxy.Pages
{
    public class DynamicsModel : PageModel
    {
        public string AuthorizationCode { get; set; }

        public void OnGet()
        {
            AuthorizationCode = Request.Query["code"];
        }
    }
}
