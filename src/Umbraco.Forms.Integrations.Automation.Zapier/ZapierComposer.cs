using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

namespace Umbraco.Forms.Integrations.Automation.Zapier
{
    public class ZapierComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<IMappingService, MappingService>(Lifetime.Singleton);
        }
    }
}
