using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetBackend.Models.Services
{
    public class HttpClientService : IHttpClientService
    {
        private static readonly HttpClient _httpClient;

        static HttpClientService()
        {
            _httpClient = new HttpClient();
            //_httpClient.Timeout = TimeSpan.FromSeconds(100);
        }

        public async Task<T> PostAsync<T>(string url, object data)
        {
            var response = await _httpClient.PostAsJsonAsync(url, data);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<T>();
            }
            throw new HttpRequestException(response.StatusCode.ToString());
        }
    }
}