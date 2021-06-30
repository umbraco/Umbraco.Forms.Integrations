using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Forms.Extensions.Crm.Hubspot.Models.Responses;
using Umbraco.Forms.Extensions.Crm.Hubspot.Services;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Umbraco.Forms.Extensions.Crm.Hubspot.Controllers
{
    [PluginController("FormsExtensions")]
    public class HubspotController : UmbracoAuthorizedJsonController
    {
        private readonly IContactService _contactService;

        public HubspotController(IContactService contactService)
        {
            _contactService = contactService;
        }

        /// <summary>
        /// ~/Umbraco/[YourAreaName]/[YourControllerName]
        /// ~/Umbraco/FormsExtensions/Hubspot/GetAllProperties
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Property>> GetAllProperties() => await _contactService.GetContactProperties();
    }
}
