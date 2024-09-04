using Newtonsoft.Json.Linq;

namespace Microfichas_App.Services
{
    public interface IAzureService
    {
        Task<JObject> GetTokenAsync(string email);
        Task<JObject> PublishDocumentAsync(string token, string fileName, int fileType, string fileContentBase64);
    }
}
