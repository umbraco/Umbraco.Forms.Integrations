using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services
{
    public interface IAccountService
    {
        Task<AccountCollectionResponseDto> Get();

        Task<AccountContactRequestDto> CreateAssociation(int accountId, int contactId);
    }
}
