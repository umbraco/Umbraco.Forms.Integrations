using System.IO;
using System.Text;

namespace Umbraco.Forms.Integrations.Commerce.Emerchantpay.Configuration;

/// <summary>
/// Override encoding to ensure request object is utf-8 encoded.
/// </summary>
public class PaymentProviderStringWriter : StringWriter
{
    public override Encoding Encoding => Encoding.UTF8;
}
