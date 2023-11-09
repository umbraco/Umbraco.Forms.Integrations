
namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.ExtensionMethods;

public static class StringExtensions
{
    public static bool IsContentValid(this string str, string type, out string error)
    {
        bool isValid = !string.IsNullOrEmpty(str) && str != "null";

        error = isValid ? string.Empty : $"{type} field is required";

        return isValid;
    }
}
