
namespace Umbraco.Cms.Integrations.Authorization.Core.Models.Dtos
{
    public class ResponseDto
    {
        public int StatusCode { get; set; }

        public string ContentType { get; set; }

        public long? ContentLength { get; set; }

        public string Content { get; set; }
    }
}
