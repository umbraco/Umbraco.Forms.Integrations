using System;
using System.Collections.Generic;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Validators
{
    public abstract class WorkflowBaseValidator<T>
        where T : class
    {
        public abstract bool IsValid(T input, ref List<Exception> exceptions);
    }
}
