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
            if (HttpContext.Current == null) throw new InvalidOperationException("HttpContext is null");
            var urlHelper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));
            e.Add("Umbraco.Forms.Extensions.HubSpot", new Dictionary<string, object>
            {
                {"GetPropertiesBaseUrl", urlHelper.GetUmbracoApiServiceBaseUrl<HubspotController>(controller => controller.GetAllProperties(null))},
            });
        }

        public void Terminate()
        {
            ServerVariablesParser.Parsing -= ServerVariablesParser_Parsing;
        }
    }
}
