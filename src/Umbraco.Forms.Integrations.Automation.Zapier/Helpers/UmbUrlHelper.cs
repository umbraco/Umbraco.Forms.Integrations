#if NETCOREAPP
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common;
using Umbraco.Extensions;
#else
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
#endif

namespace Umbraco.Forms.Integrations.Automation.Zapier.Helpers
{
    public class UmbUrlHelper
    {
#if NETCOREAPP
        private readonly IUmbracoHelperAccessor _umbracoHelperAccessor;

        private readonly IPublishedUrlProvider _publishedUrlProvider;

        public UmbUrlHelper(IUmbracoHelperAccessor umbracoHelperAccessor, IPublishedUrlProvider publishedUrlProvider)
        {
            _umbracoHelperAccessor = umbracoHelperAccessor;

            _publishedUrlProvider = publishedUrlProvider;
        }
#else
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public UmbUrlHelper(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }
#endif

        public string GetPageUrl(int umbracoPageId)
        {
#if NETCOREAPP
            string pageUrl = string.Empty;
            if (_umbracoHelperAccessor.TryGetUmbracoHelper(out UmbracoHelper umbracoHelper))
            {
                IPublishedContent publishedContent = umbracoHelper.Content(umbracoPageId);
                if (publishedContent != null)
                {
                    pageUrl = publishedContent.Url(_publishedUrlProvider, mode: UrlMode.Absolute);
                }
            }
#else
            UmbracoContext umbracoContext = _umbracoContextAccessor.UmbracoContext;
            
            var pageUrl = umbracoContext.UrlProvider.GetUrl(umbracoPageId, UrlMode.Absolute);
#endif

            return pageUrl;
        }
    }
}
