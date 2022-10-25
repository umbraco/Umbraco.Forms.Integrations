
namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Configuration
{
    public class ActiveCampaignSettings
    {
        public ActiveCampaignSettings()
        {
            ContactFields = new List<ContactFieldSettings>();
        }

        public string BaseUrl { get; }

        public string ApiKey { get; }

        public bool AllowContactUpdate { get; }

        public List<ContactFieldSettings> ContactFields { get; set; }
    }
}
