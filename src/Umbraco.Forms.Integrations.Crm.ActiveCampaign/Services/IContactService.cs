using Umbraco.Forms.Integrations.Crm.ActiveCampaign.Models.Dtos;

namespace Umbraco.Forms.Integrations.Crm.ActiveCampaign.Services;

public interface IContactService
{
    Task<string> CreateOrUpdate(ContactDetailDto contactRequestDto, bool update = false);

    Task<ContactCollectionResponseDto> Get(string email);

    Task<CustomFieldCollectionResponseDto> GetCustomFields();
}
