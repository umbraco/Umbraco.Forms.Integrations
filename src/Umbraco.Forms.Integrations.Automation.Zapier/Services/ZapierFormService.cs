using System;
using System.Collections.Generic;
using System.Linq;

using Umbraco.Forms.Core.Models;
using Umbraco.Forms.Core.Services;
using Umbraco.Forms.Integrations.Automation.Zapier.Models.Dtos;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Services;

public class ZapierFormService
{
    private readonly IFormService _formService;

    public ZapierFormService(IFormService formService)
    {
        _formService = formService;
    }

    public IEnumerable<FormDto> GetAll()
    {
        var forms = _formService.Get();

        return forms.Select(p => new FormDto
        {
            Id = p.Id.ToString(),
            Name = p.Name
        });
    }

    public Form GetById(string id) => _formService.Get().FirstOrDefault(p => p.Id == Guid.Parse(id));
}
