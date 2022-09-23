using System.Collections.Generic;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Configuration
{
    public interface ISettingsParser
    {
        IEnumerable<string> AsEnumerable(string propertyName);

        Dictionary<string, string> AsDictionary(string propertyName);
    }
}
