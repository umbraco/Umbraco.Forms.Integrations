using System.Xml.Serialization;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos;

public class PaymentDto : ResponseDto
{
    [XmlElement("unique_id")]
    public string UniqueId { get; set; }

    [XmlElement("transaction_id")]
    public string TransactionId { get; set; }

    [XmlElement("usage")]
    public string Usage { get; set; }

    [XmlElement("amount")]
    public int Amount { get; set; }

    [XmlElement("currency")]
    public string Currency { get; set; }

    [XmlElement("consumer_id")]
    public string ConsumerId { get; set; }

    [XmlElement("customer_email")]
    public string CustomerEmail { get; set; }

    [XmlElement("customer_phone")]
    public string CustomerPhone { get; set; }

    [XmlElement("billing_address")]
    public AddressDto BillingAddress { get; set; }

    [XmlElement("transaction_types")]
    public TransactionTypeDto TransactionTypes { get; set; }

    [XmlElement("business_attributes")]
    public BusinessAttribute BusinessAttribute { get; set; }

    [XmlElement("notification_url")]
    public string NotificationUrl { get; set; }

    [XmlElement("return_success_url")]
    public string ReturnSuccessUrl { get; set; }

    [XmlElement("return_failure_url")]
    public string ReturnFailureUrl { get; set; }

    [XmlElement("return_cancel_url")]
    public string ReturnCancelUrl { get; set; }

    [XmlElement("redirect_url")]
    public string RedirectUrl { get; set; }
}
