using System.Collections.Generic;
using Umbraco.Forms.Integrations.Automation.Zapier.Extensions;
using Umbraco.Forms.Integrations.Automation.Zapier.Services;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Controllers;

/// <summary>
/// When a Zapier user creates a "New Form Submitted" trigger, they are authenticated, they select a form, and the API provides an output json with the
/// structure of the selected form.
/// </summary>
public class FormPollingController : ZapierFormAuthorizedApiController
{
    private readonly ZapierFormService _zapierFormService;

    public FormPollingController(
        ZapierFormService zapierFormService,
        IUserValidationService userValidationService)
        : base(userValidationService) => _zapierFormService = zapierFormService;

    public List<Dictionary<string, string>> GetFormPropertiesById(string id)
    {
        if (!IsAccessValid()) return new List<Dictionary<string, string>>();

        var form = _zapierFormService.GetById(id);

        if (form == null) return new List<Dictionary<string, string>>();

        return new List<Dictionary<string, string>> { form.ToEmptyFormDictionary() };
    }
}
