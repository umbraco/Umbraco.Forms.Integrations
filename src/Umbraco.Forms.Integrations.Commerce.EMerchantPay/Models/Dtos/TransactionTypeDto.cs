using System.Collections.Generic;
using System.Xml.Serialization;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Models.Dtos
{
    public class TransactionTypeDto
    {
        [XmlElement("transaction_type")]
        public List<TransactionTypeRecordDto> TransactionTypes { get; set; }
    }

    public class TransactionTypeRecordDto
    {
        [XmlAttribute("name")]
        public string TransactionType { get; set; }
    }
}
