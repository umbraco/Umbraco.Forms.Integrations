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
    public class MappingService: IMappingService
    {
        private readonly Dictionary<string, string> _content;

        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public MappingService(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;

            _content = new Dictionary<string, string>();
        }

        public IMappingService IncludeFieldsMappings(string mappings, RecordEventArgs e)
        {
            var mappingsList = JsonConvert.DeserializeObject<List<FieldMapping>>(mappings);
            if (mappingsList.Any())
            {
                foreach (var mapping in mappingsList)
                {
                    var fieldRecord = e.Record.RecordFields[Guid.Parse(mapping.Value)];
                    _content.Add(mapping.Alias, string.IsNullOrEmpty(mapping.StaticValue) ? fieldRecord.ValuesAsString() : mapping.StaticValue);
                }
            }

            return this;
        }

        public IMappingService IncludeStandardFieldsMappings(string mappings, RecordEventArgs e)
        {
            if (!string.IsNullOrEmpty(mappings))
            {
                var standardFieldMappings =
                    JsonConvert.DeserializeObject<IEnumerable<StandardFieldMapping>>(mappings,
                        FormsJsonSerializerSettings.Default).ToList();

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
