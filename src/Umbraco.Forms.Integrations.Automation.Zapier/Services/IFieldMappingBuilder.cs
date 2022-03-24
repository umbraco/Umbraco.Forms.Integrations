using System.Collections.Generic;

using Umbraco.Forms.Core;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Services
{
    public interface IFieldMappingBuilder
    {
        IFieldMappingBuilder IncludeFieldsMappings(string mappings, RecordEventArgs e);

        IFieldMappingBuilder IncludeStandardFieldsMappings(string mappings, RecordEventArgs e);

        Dictionary<string, string> Map();
    }
}
