using System.Runtime.Serialization;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Models.Dtos
{
    public class HubspotWorkflowFormFieldDto
    {
        [DataMember(Name = "caption")]
        public string Caption { get; set; } = string.Empty;

        [DataMember(Name = "id")]
        public Guid Id { get; set; }
    }
}
