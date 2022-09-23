using System;

using Umbraco.Forms.Core.Persistence.Dtos;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Helpers
{
    public class FormHelper
    {
        protected Record Record { get; set; }

        public FormHelper(Record record)
        {
            Record = record;
        }

        public Guid GetFormId() => Record.Form;

        public Guid GetRecordUniqueId() => Record.UniqueId;

        public string GetRecordFieldValue(string key) => Record.RecordFields[Guid.Parse(key)].ValuesAsString();

        public void UpdateRecordFieldValue(string key, string value)
        {
            Record.RecordFields[Guid.Parse(key)].Values.Clear();

            Record.RecordFields[Guid.Parse(key)].Values.Add(value);
        }
    }
}
