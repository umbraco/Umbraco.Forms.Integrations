
using System.Collections.Generic;
using Umbraco.Forms.Core;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Services
{
    public interface IMappingService
    {
        IMappingService IncludeFieldsMappings(string mappings, RecordEventArgs e);

        IMappingService IncludeStandardFieldsMappings(string mappings, RecordEventArgs e);

        Dictionary<string, string> Map();
    }
}
