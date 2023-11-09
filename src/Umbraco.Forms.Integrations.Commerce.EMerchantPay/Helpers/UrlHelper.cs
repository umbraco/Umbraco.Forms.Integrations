using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common;
using Umbraco.Extensions;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Helpers;

public class UrlHelper
{
    private readonly IUmbracoHelperAccessor _umbracoHelperAccessor;

    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public UrlHelper(IUmbracoHelperAccessor umbracoHelperAccessor, IPublishedUrlProvider publishedUrlProvider)
    {
        _umbracoHelperAccessor = umbracoHelperAccessor;

        _publishedUrlProvider = publishedUrlProvider;
    }

    public string GetPageUrl(int pageId)
    {
        string pageUrl = string.Empty;
        if (_umbracoHelperAccessor.TryGetUmbracoHelper(out UmbracoHelper umbracoHelper))
        {
            IPublishedContent publishedContent = umbracoHelper.Content(pageId);
            if (publishedContent != null)
            {
                pageUrl = publishedContent.Url(_publishedUrlProvider, mode: UrlMode.Absolute);
            }
        }

        return pageUrl;
    }
}
