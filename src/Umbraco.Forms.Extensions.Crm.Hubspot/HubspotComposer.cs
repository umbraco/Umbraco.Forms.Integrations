using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Forms.Extensions.Crm.Hubspot.Services;

namespace Umbraco.Forms.Extensions.Crm.Hubspot
{
    public class HubspotComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<IContactService, HubspotContactService>(Lifetime.Singleton);

            composition.Components().Append<HubspotComponent>();
        }
    }
}
