using System;
using System.Collections.Generic;
using System.Linq;

#if NETCOREAPP
using Umbraco.Forms.Core.Services;
#else
using Umbraco.Forms.Core.Data.Storage;
#endif

using Umbraco.Forms.Integrations.Automation.Zapier.Models.Dtos;
using Umbraco.Forms.Core.Models;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Services
{
    public class ZapierFormService
    {
#if NETCOREAPP
        private readonly IFormService _formService;

        public ZapierFormService(IFormService formService)
        {
            _formService = formService;
        }
#else
        private readonly IFormStorage _formStorage;

        public ZapierFormService(IFormStorage formStorage)
        {
            _formStorage = formStorage;
        }
#endif

        /// <summary>
        /// for V8 use IFormStorage to retrieve forms saved on disk also
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FormDto> GetAll()
        {
#if NETCOREAPP
            var forms = _formService.Get();
#else
            var forms = _formStorage.GetAll();
#endif

            return forms.Select(p => new FormDto
            {
                Id = p.Id.ToString(),
                Name = p.Name
            });
        }

        public Form GetById(string id)
        {
#if NETCOREAPP
            return _formService.Get().FirstOrDefault(p => p.Id == Guid.Parse(id));
#else
            return _formStorage.GetAll().FirstOrDefault(p => p.Id == Guid.Parse(id));
#endif
        }
    }
}
