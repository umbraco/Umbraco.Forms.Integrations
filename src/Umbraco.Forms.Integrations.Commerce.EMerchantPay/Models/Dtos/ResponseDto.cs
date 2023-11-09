using System.Xml.Serialization;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos;

public abstract class ResponseDto
{
    [XmlElement("status")]
    public string Status { get; set; }

    [XmlElement("code")]
    public string Code { get; set; }

    [XmlElement("message")]
    public string Message { get; set; }

    [XmlElement("technical_message")]
    public string TechnicalMessage { get; set; }

}
