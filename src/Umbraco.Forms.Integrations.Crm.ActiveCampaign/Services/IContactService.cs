using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services
{
    public interface IContactService
    {
        Task<bool> CreateOrUpdate(ContactRequestDto contactRequestDto, bool update = false);

        Task<ContactCollectionResponseDto> Get(string email);
    }
}
