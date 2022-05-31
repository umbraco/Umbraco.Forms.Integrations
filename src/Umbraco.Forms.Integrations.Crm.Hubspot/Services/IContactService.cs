using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models.Dtos;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models.Responses;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Services
{
    public enum CommandResult
    {
        Success,
        Failed,
        NotConfigured,
    }

    public interface IContactService
    {
        AuthenticationMode IsAuthorizationConfigured();

        string GetAuthenticationUrl();

        Task<AuthorizationResult> AuthorizeAsync(string code);

        AuthorizationResult Deauthorize();

        Task<IEnumerable<Property>> GetContactPropertiesAsync();

        Task<CommandResult> PostContactAsync(Record record, List<MappedProperty> fieldMappings, Dictionary<string, string> additionalFields);
    }
}
