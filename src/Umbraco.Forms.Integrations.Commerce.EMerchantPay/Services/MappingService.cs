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

            if(result.Count != configMappings.Count()) return false;

            foreach(var configMapping in configMappings)
            {
                if (!result.Any(p => p.CustomerProperty == configMapping)) return false;
            }

            return true;
        }
    }
}
