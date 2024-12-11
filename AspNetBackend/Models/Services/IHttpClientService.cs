using System.Threading.Tasks;

namespace AspNetBackend.Models.Services
{
    public interface IHttpClientService
    {
        Task<T> PostAsync<T>(string url, object data);
    }
}
