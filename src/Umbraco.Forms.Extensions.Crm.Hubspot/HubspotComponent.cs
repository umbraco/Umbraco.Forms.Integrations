using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Composing;
using Umbraco.Web;
using Umbraco.Web.JavaScript;

namespace Umbraco.Forms.Extensions.Crm.Hubspot
{
    public class HubspotComposer : ComponentComposer<HubspotComponent>, IUserComposer { }

    public class HubspotComponent : IComponent
    {
        public void Initialize()
        {
            ServerVariablesParser.Parsing += ServerVariablesParser_Parsing;
        }

        private void ServerVariablesParser_Parsing(object sender, Dictionary<string, object> e)
        {
            if (!e.ContainsKey("umbracoUrls"))
                throw new ArgumentException("Missing umbracoUrls.");

            var umbracoUrlsObject = e["umbracoUrls"];
            if (umbracoUrlsObject == null)
                throw new ArgumentException("Null umbracoUrls");

            if (!(umbracoUrlsObject is Dictionary<string, object> umbracoUrls))
                throw new ArgumentException("Invalid umbracoUrls");

            if (HttpContext.Current == null) throw new InvalidOperationException("HttpContext is null");
            var urlHelper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));

            umbracoUrls["umbracoFormsExtensionsHubspotBaseUrl"] = urlHelper.GetUmbracoApiServiceBaseUrl<HubspotController>(controller => controller.GetAllProperties(null));
        }

        public void Terminate()
        {
            ServerVariablesParser.Parsing -= ServerVariablesParser_Parsing;
        }
    }
}
