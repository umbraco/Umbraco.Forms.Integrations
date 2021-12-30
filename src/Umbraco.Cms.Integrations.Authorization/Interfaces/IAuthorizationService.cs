using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Umbraco.Cms.Integrations.Authorization.Core.Models.Dtos;

namespace Umbraco.Cms.Integrations.Authorization.Core.Interfaces
{
    public interface IAuthorizationService
    {
        Task<ResponseDto> ProcessAsync(HttpContent content);

        HttpContent GetContent(IFormCollection form);
    }
}
