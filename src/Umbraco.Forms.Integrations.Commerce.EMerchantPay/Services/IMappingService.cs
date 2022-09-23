using System.Collections.Generic;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Services
{
    public interface IMappingService<Mapping>
    {
        bool TryParse(string mappings, out List<Mapping> result);
    }
}
