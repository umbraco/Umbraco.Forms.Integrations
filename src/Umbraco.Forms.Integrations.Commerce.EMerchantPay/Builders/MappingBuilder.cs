using System;
using System.Collections.Generic;
using System.Linq;

using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Models.Dtos;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Builders
{
    public class MappingBuilder
    {
        protected MappingValues Values = new MappingValues();

        public MappingBuilder SetValues(Record record, List<Mapping> mappings)
        {
            Values.Email = GetValue(nameof(MappingValues.Email), record, mappings);
            Values.FirstName = GetValue(nameof(MappingValues.FirstName), record, mappings);
            Values.LastName = GetValue(nameof(MappingValues.LastName), record, mappings);
            Values.Address = GetValue(nameof(MappingValues.Address), record, mappings);
            Values.ZipCode = GetValue(nameof(MappingValues.ZipCode), record, mappings);
            Values.City = GetValue(nameof(MappingValues.City), record, mappings);
            Values.State = GetValue(nameof(MappingValues.State), record, mappings);
            Values.Country = GetValue(nameof(MappingValues.Country), record, mappings);
            Values.Phone = GetValue(nameof(MappingValues.Phone), record, mappings);

            return this;
        }

        public MappingBuilder SetSuccessUrl()
        {
            return this;
        }

        private string GetValue(string property, Record record, List<Mapping> mappings)
        {
            var key = mappings.First(p => p.CustomerProperty == nameof(property)).Field.Id;
            return record.RecordFields[Guid.Parse(key)].ValuesAsString();
        }

        public MappingValues Build() => Values;
    }
}
