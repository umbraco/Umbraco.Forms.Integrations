using System.Collections.Generic;
using System.Threading.Tasks;

using Umbraco.Forms.Integrations.Automation.Zapier.Services;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Helpers;

public class TriggerHelper
{
    private readonly ZapierService _zapierService;

    public TriggerHelper(ZapierService zapierService)
    {
        _zapierService = zapierService;
    }

    public string FormExecute(string hookUrl, Dictionary<string, string> content)
    {
        var t = Task.Run(
            async () => await _zapierService.TriggerAsync(hookUrl, content));

        return t.Result;
    }
}
