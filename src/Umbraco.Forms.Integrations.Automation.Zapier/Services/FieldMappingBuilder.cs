using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using Umbraco.Core.Models.PublishedContent;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Providers.Models;
using Umbraco.Web;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Services
{
    /// <summary>
    /// Fluent Builder for packing fields mappings.
    /// </summary>
    public class FieldMappingBuilder: IFieldMappingBuilder
    {
        private readonly Dictionary<string, string> _content;

        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public FieldMappingBuilder(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;

            _content = new Dictionary<string, string>();
        }

        /// <summary>
        /// Add form fields to the request content sent to Zapier. If no propery is mapped, all fields will be included by their alias.
        /// </summary>
        /// <param name="mappings">Serialized details of the form fields</param>
        /// <param name="e">Form record details</param>
        /// <returns></returns>
        public IFieldMappingBuilder IncludeFieldsMappings(string mappings, RecordEventArgs e)
        {
            var mappingsList = JsonConvert.DeserializeObject<List<FieldMapping>>(mappings);
            if (mappingsList.Any())
            {
                foreach (var mapping in mappingsList)
                {
                    var fieldRecord = !string.IsNullOrEmpty(mapping.Value) ? e.Record.RecordFields[Guid.Parse(mapping.Value)] : null;

                    _content.Add(mapping.Alias, 
                        fieldRecord != null 
                            ? string.IsNullOrEmpty(mapping.StaticValue) 
                                ? fieldRecord.ValuesAsString() : mapping.StaticValue
                            : mapping.StaticValue);
                }
            }
            else
            {
                foreach (var recordField in e.Record.RecordFields)
                {
                    _content.Add(recordField.Value.Alias, recordField.Value.ValuesAsString());
                }
            }

            return this;
        }

        /// <summary>
        /// Add form standard fields - that are set as Included in the settings of the workflow - to the request content sent to Zapier.
        /// </summary>
        /// <param name="mappings">Serialized details of the form standard fields</param>
        /// <param name="e">Form record details</param>
        /// <returns></returns>
        public IFieldMappingBuilder IncludeStandardFieldsMappings(string mappings, RecordEventArgs e)
        {
            if (!string.IsNullOrEmpty(mappings))
            {
                var standardFieldMappings =
                    JsonConvert.DeserializeObject<IEnumerable<StandardFieldMapping>>(mappings).ToList();

                foreach (StandardFieldMapping fieldMapping in standardFieldMappings.Where(x => x.Include))
                {
                    switch (fieldMapping.Field)
                    {
                        case StandardField.FormId:
                            _content.Add(fieldMapping.KeyName, e.Form.Id.ToString());
                            break;
                        case StandardField.FormName:
                            _content.Add(fieldMapping.KeyName, e.Form.Name);
                            break;
                        case StandardField.PageUrl:
                            UmbracoContext umbracoContext = _umbracoContextAccessor.UmbracoContext;
                            var pageUrl = umbracoContext.UrlProvider.GetUrl(e.Record.UmbracoPageId, UrlMode.Absolute);
                            _content.Add(fieldMapping.KeyName, pageUrl);
                            break;
                        case StandardField.SubmissionDate:
                            _content.Add(fieldMapping.KeyName, e.Record.Created.ToString());
                            break;
                        default:
                            throw new InvalidOperationException(
                                $"The field '{fieldMapping.Field}' is not supported for including in the collection.");
                    }
                }
            }

            return this;
        }

        public Dictionary<string, string> Map() => _content;
    }
}
