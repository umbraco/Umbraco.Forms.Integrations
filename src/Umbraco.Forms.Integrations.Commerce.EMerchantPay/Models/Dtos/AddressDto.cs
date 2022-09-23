using System.Xml.Serialization;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos
{
    public class AddressDto
    {
        [XmlElement("first_name")]
        public string FirstName { get; set; }

        [XmlElement("last_name")]
        public string LastName { get; set; }

        [XmlElement("address1")]
        public string Address1 { get; set; }

        [XmlElement("address2")]
        public string Address2 { get; set; }

        [XmlElement("zip_code")]
        public string ZipCode { get; set; }

        [XmlElement("city")]
        public string City { get; set; }

        [XmlElement("state")]
        public string State { get; set; }

        [XmlElement("country")]
        public string Country { get; set; }
    }
}
