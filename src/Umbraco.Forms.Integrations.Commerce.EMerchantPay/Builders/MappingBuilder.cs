using System;
using System.Collections.Generic;
using System.Linq;

using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Builders;

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

    private string GetValue(string propertyName, Record record, List<Mapping> mappings)
    {
        var mapping = mappings.FirstOrDefault(p => p.CustomerProperty == propertyName);

        return mapping != null
            ? record.RecordFields[Guid.Parse(mapping.Field.Id)].ValuesAsString()
            : string.Empty;
    }

    public MappingValues Build() => Values;
}
