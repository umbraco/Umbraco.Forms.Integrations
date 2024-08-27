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
                { Constants.FormProperties.Id, form.Id.ToString() },
                { Constants.FormProperties.Name, form.Name },
                { Constants.FormProperties.SubmissionDate, DateTime.UtcNow.ToString("s") },
                { Constants.FormProperties.PageUrl, pageUrl }
            };

            foreach (var recordField in record.RecordFields)
            {
                contentDict.Add(recordField.Value.Alias, recordField.Value.ValuesAsString());
            }

            return contentDict;
        }

        public static Dictionary<string, string> ToEmptyFormDictionary(this Form form)
        {
            var contentDict = new Dictionary<string, string>
            {
                { Constants.FormProperties.Id, form.Id.ToString() },
                { Constants.FormProperties.Name, form.Name },
                { Constants.FormProperties.SubmissionDate, DateTime.UtcNow.ToString("s") },
                { Constants.FormProperties.PageUrl, string.Empty }
            };

            foreach (var field in form.AllFields)
            {
                contentDict.Add(field.Alias, string.Empty);
            }

            return contentDict;
        }

        public static Dictionary<string, string> ToFormDictionary(this Form form)
        {
            var contentDict = new Dictionary<string, string>
            {
                { Constants.FormProperties.Id, form.Id.ToString() },
                { Constants.FormProperties.Name, form.Name },
                { Constants.FormProperties.SubmissionDate, DateTime.UtcNow.ToString("s") },
                { Constants.FormProperties.PageUrl, string.Empty }
            };

            foreach (var field in form.AllFields)
            {
                contentDict.Add(field.Alias, string.Empty);
            }

            return contentDict;
        }
    }
}
