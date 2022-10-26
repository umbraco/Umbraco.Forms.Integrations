
namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Configuration
{
    public class ActiveCampaignSettings
    {
        public ActiveCampaignSettings()
        {
            ContactFields = new List<ContactFieldSettings>();
        }

        public string BaseUrl { get; set; }

        public string ApiKey { get; set; }

        public bool AllowContactUpdate { get; set; }

        public List<ContactFieldSettings> ContactFields { get; set; }
    }
}
