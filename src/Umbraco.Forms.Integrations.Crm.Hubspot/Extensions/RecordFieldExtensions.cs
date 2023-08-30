using Umbraco.Forms.Core.Data.Helpers;
using Umbraco.Forms.Core.Persistence.Dtos;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Extensions;

public static class RecordFieldExtensions
{
    public static string ValuesAsHubspotString(this RecordField recordField, bool escaped = true)
    {
        if (!recordField.HasValue())
        {
            return string.Empty;
        }

        if (!escaped)
        {
            return string.Join(", ", recordField.Values.ToArray());
        }

        return string.Join(", ", recordField.Values.ConvertAll((object input) => JsonHelper.EscapeStringValue(input.ToString())).ToArray());
    }
}
