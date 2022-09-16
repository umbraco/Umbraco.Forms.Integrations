using System.IO;
using System.Net.Http;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using Umbraco.Forms.Integrations.Commerce.EMerchantPay.Configuration;

namespace Umbraco.Forms.Integrations.Commerce.EMerchantPay.Services
{
    public abstract class BaseService<T> where T : class
    {
        protected StringContent Serialize(T input, string root)
        {
            var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(root));

            using (var stringWriter = new PaymentProviderStringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(stringWriter))
                {
                    serializer.Serialize(writer, input);

                    return new StringContent(stringWriter.ToString(), Encoding.UTF8, "text/xml");
                }
            }
        }

        protected T Deserialize(string response, string root)
        {
            var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(root));

            using (var stringReader = new StringReader(response))
            {
                return (T)serializer.Deserialize(stringReader);
            }
        }

    }
}
