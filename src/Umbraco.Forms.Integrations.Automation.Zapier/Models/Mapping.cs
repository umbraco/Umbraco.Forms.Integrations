using Newtonsoft.Json;

namespace Umbraco.Forms.Integrations.Automation.Zapier.Models
{
    public class Mapping
    {
        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("staticValue")]
        public string StaticValue { get; set; }
    }
}
