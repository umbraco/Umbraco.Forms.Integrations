using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;


using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Models;
using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Core.Providers.Models;

#if NETCOREAPP
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.UmbracoContext;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common;
using Umbraco.Extensions;
using Umbraco.Cms.Core;
#else
using Umbraco.Web;
using Umbraco.Forms.Core.Providers.Models;
using Umbraco.Core.Models.PublishedContent;
#endif

namespace Umbraco.Forms.Integrations.Automation.Zapier.Services
{
    /// <summary>
    /// Fluent Builder for packing fields mappings.
    /// </summary>
    public class FieldMappingBuilder: IFieldMappingBuilder
    {
        private readonly Dictionary<string, string> _content;



#if NETCOREAPP
        private readonly IUmbracoHelperAccessor _umbracoHelperAccessor;

        private readonly IPublishedUrlProvider _publishedUrlProvider;

        public FieldMappingBuilder(IUmbracoHelperAccessor umbracoHelperAccessor, IPublishedUrlProvider publishedUrlProvider)
        {
            _umbracoHelperAccessor = umbracoHelperAccessor;

            _publishedUrlProvider = publishedUrlProvider;

            _content = new Dictionary<string, string>();
        }
#else
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public FieldMappingBuilder(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;

            _content = new Dictionary<string, string>();
        }

#endif


        /// <summary>
        /// Add form fields to the request content sent to Zapier. If no propery is mapped, all fields will be included by their alias.
        /// </summary>
        /// <param name="mappings">Serialized details of the form fields</param>
        /// <param name="e">Form record details</param>
        /// <returns></returns>
        public IFieldMappingBuilder IncludeFieldsMappings(string mappings, Record record)
        {
            var mappingsList = JsonConvert.DeserializeObject<List<FieldMapping>>(mappings);
            if (mappingsList.Any())
            {
                foreach (var mapping in mappingsList)
                {
                    var fieldRecord = !string.IsNullOrEmpty(mapping.Value) ? record.RecordFields[Guid.Parse(mapping.Value)] : null;

                    _content.Add(mapping.Alias, 
                        fieldRecord != null 
                            ? string.IsNullOrEmpty(mapping.StaticValue) 
                                ? fieldRecord.ValuesAsString() : mapping.StaticValue
                            : mapping.StaticValue);
                }
            }
            else
            {
                foreach (var recordField in record.RecordFields)
                {
                    _content.Add(recordField.Value.Alias, recordField.Value.ValuesAsString());
                }
            }

            return this;
        }

        private readonly IUmbracoContextFactory _umbracoContextFactory;

        /// <summary>
        /// Add form standard fields - that are set as Included in the settings of the workflow - to the request content sent to Zapier.
        /// </summary>
        /// <param name="mappings">Serialized details of the form standard fields</param>
        /// <param name="e">Form record details</param>
        /// <returns></returns>
        public IFieldMappingBuilder IncludeStandardFieldsMappings(string mappings, Record record, Form form)
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
                            _content.Add(fieldMapping.KeyName, form.Id.ToString());
                            break;
                        case StandardField.FormName:
                            _content.Add(fieldMapping.KeyName, form.Name);
                            break;
                        case StandardField.PageUrl:
#if NETCOREAPP
                            if (_umbracoHelperAccessor.TryGetUmbracoHelper(out UmbracoHelper umbracoHelper))
                            {
                                IPublishedContent publishedContent = umbracoHelper.Content(record.UmbracoPageId);
                                if (publishedContent != null)
                                {
                                    var pageUrl = publishedContent.Url(_publishedUrlProvider, mode: UrlMode.Absolute);
                                    _content.Add(fieldMapping.KeyName, pageUrl);
                                }
                            }
#else
                            UmbracoContext umbracoContext = _umbracoContextAccessor.UmbracoContext;
                            var pageUrl = umbracoContext.UrlProvider.GetUrl(record.UmbracoPageId, UrlMode.Absolute);
                            _content.Add(fieldMapping.KeyName, pageUrl);
#endif
                                break;
                        case StandardField.SubmissionDate:
                            _content.Add(fieldMapping.KeyName, record.Created.ToString());
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
