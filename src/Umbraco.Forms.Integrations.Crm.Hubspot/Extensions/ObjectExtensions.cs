using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Converts an object to a dictionary of it's properties.
        /// </summary>
        /// <remarks>
        /// Hat-tip: https://stackoverflow.com/a/4944547
        /// </remarks>
        public static T ToObject<T>(this IDictionary<string, object> source)
                where T : class, new()
        {
            var obj = new T();
            var type = obj.GetType();

            foreach (var item in source)
            {
                type
                    .GetProperty(item.Key)
                    .SetValue(obj, item.Value, null);
            }

            return obj;
        }

        /// <summary>
        /// Converts an dictionary of properties to an object.
        /// </summary>
        /// <remarks>
        /// Hat-tip: https://stackoverflow.com/a/4944547
        /// </remarks>
        public static IDictionary<string, string> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance) =>
            source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => GetPropertyName(propInfo),
                propInfo => (propInfo.GetValue(source, null)?.ToString())
            );

        private static string GetPropertyName(PropertyInfo propInfo)
        {
            var jsonPropertyAttribute = ((JsonPropertyAttribute[])propInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), true)).SingleOrDefault();
            if (jsonPropertyAttribute == null)
            {
                return propInfo.Name;
            }

            return jsonPropertyAttribute.PropertyName;
        }
    }
}
