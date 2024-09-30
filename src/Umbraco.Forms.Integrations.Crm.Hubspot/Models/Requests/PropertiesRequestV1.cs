using System.Text.Json.Serialization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot
{
    internal class PropertiesRequestV1
    {
        [JsonPropertyName("properties")]
        public IList<PropertyValue> Properties { get; set; } = new List<PropertyValue>();

        internal class PropertyValue
        {
            public PropertyValue(string property, string value)
            {
                Property = property;
                Value = value;
            }

            [JsonPropertyName("property")]
            public string Property { get; }

            [JsonPropertyName("value")]
            public string Value { get; }
        }
    }
}
