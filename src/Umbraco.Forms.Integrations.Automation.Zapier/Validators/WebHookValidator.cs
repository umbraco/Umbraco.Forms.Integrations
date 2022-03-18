using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Validators
{
    public class WebHookValidator : WorkflowBaseValidator<string>
    {
        private const string Pattern =
            "((http|https)://)(www.)?[a-zA-Z0-9@:%._\\+~#?&//=]{2,256}\\.[a-z]{2,6}\\b([-a-zA-Z0-9@:%._\\+~#?&//=]*)";

        public override bool IsValid(string input, ref List<Exception> exceptions)
        {
            if (string.IsNullOrEmpty(input))
            {
                exceptions.Add(new Exception("URL is required"));
                return false;
            }

            var regex = new Regex(Pattern);

            var result = regex.IsMatch(input);

            if(!result)
                exceptions.Add(new Exception("Invalid URL format"));

            return result;
        }
    }
}
