using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Configuration;

public class SettingsParser : ISettingsParser
{
    private readonly PaymentProviderSettings _settings;

    public SettingsParser(IOptions<PaymentProviderSettings> options)
    {
        _settings = options.Value;
    }

    public Dictionary<string, string> AsDictionary(string propertyName)
    {
        var property = _settings.GetType().GetProperty(propertyName);

        if (property == null) throw new ArgumentNullException(nameof(SettingsParser));

        var propertyValues = property.GetValue(_settings) as Dictionary<string, string>;

        return propertyValues;
    }

    public IEnumerable<string> AsEnumerable(string propertyName)
    {
        var property = _settings.GetType().GetProperty(propertyName);

        if (property == null) throw new ArgumentNullException(nameof(SettingsParser));

        var propertyValues = property.GetValue(_settings);

        return propertyValues != null ? propertyValues as IEnumerable<string> : Enumerable.Empty<string>();
    }
}
