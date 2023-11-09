using Newtonsoft.Json;
using System.Collections.Generic;

namespace Umbraco.Forms.Integrations.Crm.Hubspot;

internal class PropertiesRequestV1
{
    [JsonProperty(PropertyName = "properties")]
    public IList<PropertyValue> Properties { get; set; } = new List<PropertyValue>();

    internal class PropertyValue
    {
        public PropertyValue(string property, string value)
        {
            Property = property;
            Value = value;
        }

        [JsonProperty(PropertyName = "property")]
        public string Property { get; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; }
    }
}
