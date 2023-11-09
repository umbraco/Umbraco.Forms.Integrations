using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common;
using Umbraco.Extensions;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Helpers;

public class UmbUrlHelper
{
    private readonly IUmbracoHelperAccessor _umbracoHelperAccessor;

    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public UmbUrlHelper(IUmbracoHelperAccessor umbracoHelperAccessor, IPublishedUrlProvider publishedUrlProvider)
    {
        _umbracoHelperAccessor = umbracoHelperAccessor;

        _publishedUrlProvider = publishedUrlProvider;
    }

    public string GetPageUrl(int umbracoPageId)
    {
        string pageUrl = string.Empty;
        if (_umbracoHelperAccessor.TryGetUmbracoHelper(out UmbracoHelper umbracoHelper))
        {
            IPublishedContent publishedContent = umbracoHelper.Content(umbracoPageId);
            if (publishedContent != null)
            {
                pageUrl = publishedContent.Url(_publishedUrlProvider, mode: UrlMode.Absolute);
            }
        }

        return pageUrl;
    }
}
