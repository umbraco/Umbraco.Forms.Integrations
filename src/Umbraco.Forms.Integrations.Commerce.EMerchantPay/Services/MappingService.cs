using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;

using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Helpers;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Models.Dtos;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Services
{
    public class MappingService : IMappingService<Mapping>
    {
        private readonly MappingFieldHelper _mappingFieldHelper;

        public MappingService(MappingFieldHelper mappingFieldHelper)
        {
            _mappingFieldHelper = mappingFieldHelper;
        }

        /// <summary>
        /// Validate that mappings are valid and at least email property is mapped.
        /// </summary>
        /// <param name="mappings"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool TryParse(string mappings, out List<Mapping> result)
        {
            var configMappings = _mappingFieldHelper.GetMappings();

            result = new List<Mapping>();

            if (string.IsNullOrEmpty(mappings)) return false;

            result = JsonConvert.DeserializeObject<List<Mapping>>(mappings);

            return result.Count == configMappings.Count() + 1 && result.Any(p => p.CustomerProperty == nameof(MappingValues.Email));
        }
    }
}
