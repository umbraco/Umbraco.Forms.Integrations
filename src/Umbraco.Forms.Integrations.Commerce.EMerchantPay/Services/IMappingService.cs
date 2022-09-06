using System.Collections.Generic;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Services
{
    public interface IMappingService<Mapping>
    {
        bool TryParse(string mappings, out List<Mapping> result);
    }
}
