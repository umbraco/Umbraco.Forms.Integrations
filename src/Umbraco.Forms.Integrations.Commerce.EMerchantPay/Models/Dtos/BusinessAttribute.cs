using System.Xml.Serialization;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos;

public class BusinessAttribute
{
    /// <summary>
    /// date format: dd-mm-yyyy
    /// </summary>
    [XmlElement("event_start_date")]
    public string EventStartDate { get; set; }

    [XmlElement("event_end_date")]
    public string EventEndDate { get; set; }

    [XmlElement("event_organizer_id")]
    public string EventOrganizerId { get; set; }

    [XmlElement("event_id")]
    public string EventId { get; set; }

    [XmlElement("date_of_order")]
    public string DateOfOrder { get; set; }

    [XmlElement("delivery_date")]
    public string DeliveryDate { get; set; }

    [XmlElement("name_of_the_supplier")]
    public string NameOfTheSupplier { get; set; }
}
