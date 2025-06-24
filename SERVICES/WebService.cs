using System.Text;
using System.Text.Json;
using $safeprojectname$.Models;

namespace $safeprojectname$.Services
{
    public class WebService
    {
        private readonly HttpClient _httpClient;
        private const string _baseUrl = "https://your-web-service.com/api"; // Web servisin URL'sini buraya ekleyin

        public WebService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> BarkodListesiGonder(List<BarkodModel> barkodlar)
        {
            try
            {
                string json = JsonSerializer.Serialize(barkodlar);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/barkodGonder", content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
