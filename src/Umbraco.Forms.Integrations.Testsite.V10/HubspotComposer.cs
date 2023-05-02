using System.Net.Http.Headers;

using Umbraco.Cms.Core.Composing;

namespace Umbraco.Forms.Integrations.Testsite.V10;

public class HubspotComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services
            .AddHttpClient(Constants.HubspotClient, client =>
            {
                client.BaseAddress = new Uri("https://api.hubapi.com/crm/v3/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

        builder.Services.AddScoped<HubspotPrivateAccessTokenFilterAttribute>();
    }
}
