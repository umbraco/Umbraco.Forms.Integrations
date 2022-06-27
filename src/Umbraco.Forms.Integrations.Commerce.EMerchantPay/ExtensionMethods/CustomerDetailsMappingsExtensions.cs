using System.Collections.Generic;

using Newtonsoft.Json;

using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Models.Dtos;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.ExtensionMethods
{
    public static class CustomerDetailsMappingsExtensions
    {
        /// <summary>
        /// Parse consumer mappings and validate the correct number of mappings
        /// </summary>
        /// <param name="serializedDetails"></param>
        /// <param name="mappings"></param>
        /// <returns></returns>
        public static bool TryParseMappings(this string serializedDetails, out List<Mapping> mappings)
        {
            mappings = new List<Mapping>();

            if (string.IsNullOrEmpty(serializedDetails)) return false;

            mappings = JsonConvert.DeserializeObject<List<Mapping>>(serializedDetails);

            return mappings.Count == Constants.RequiredMappingsNo;
        }
    }
}
