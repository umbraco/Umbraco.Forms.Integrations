using System.Collections.Generic;

using Umbraco.Forms.Core.Models;
using Umbraco.Forms.Core.Persistence.Dtos;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Services
{
    public interface IFieldMappingBuilder
    {
        IFieldMappingBuilder IncludeFieldsMappings(string mappings, Record record);

        IFieldMappingBuilder IncludeStandardFieldsMappings(string mappings, Record record, Form form);

        Dictionary<string, string> Map();
    }
}
