using System.Runtime.Serialization;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos
{
    /// <summary>
    /// Get form field to display in ActiveCampaign Contacts Workflow, avoid confusion with FormFieldDto.
    /// </summary>
    public class ActiveCampaignFormFieldDto
    {
        [DataMember(Name = "caption")]
        public string Caption { get; set; } = string.Empty;

        [DataMember(Name = "id")]
        public Guid Id { get; set; }
    }
}
