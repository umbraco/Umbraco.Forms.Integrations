using System;
using System.Collections.Generic;

using Umbraco.Forms.Core.Models;
using Umbraco.Forms.Core.Persistence.Dtos;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Extensions
{
    public static class FormExtensions
    {
        public static Dictionary<string, string> ToFormDictionary(this Form form, Record record, string pageUrl)
        {
            var contentDict = new Dictionary<string, string>
            {
                { Constants.Form.Id, form.Id.ToString() },
                { Constants.Form.Name, form.Name },
                { Constants.Form.SubmissionDate, DateTime.UtcNow.ToString("s") },
                { Constants.Form.PageUrl, pageUrl }
            };

            foreach (var recordField in record.RecordFields)
            {
                contentDict.Add(recordField.Value.Alias, recordField.Value.ValuesAsString());
            }

            return contentDict;
        }
    }
}
