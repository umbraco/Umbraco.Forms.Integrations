using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.ExtensionMethods;

public static class CustomerDetailsMappingsExtensions
{
    /// <summary>
    /// Parse consumer mappings and validate that at least email property is mapped.
    /// </summary>
    /// <param name="serializedDetails"></param>
    /// <param name="mappings"></param>
    /// <returns></returns>
    public static bool TryParseMappings(this string serializedDetails, out List<Mapping> mappings)
    {
        mappings = new List<Mapping>();

        if (string.IsNullOrEmpty(serializedDetails)) return false;

        mappings = JsonConvert.DeserializeObject<List<Mapping>>(serializedDetails);

        return mappings.Count > 0 && mappings.Any(p => p.CustomerProperty == nameof(MappingValues.Email));
    }
}
