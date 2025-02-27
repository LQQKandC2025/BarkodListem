using System.Text;
using System.Text.Json;
using BarkodListem.Models;
using Newtonsoft.Json;


namespace BarkodListem.Services
{
    public class WebService
    {
        private readonly HttpClient _httpClient;
        private const string _baseUrl = "https://your-web-service.com/api"; // Web servisin URL'sini buraya ekleyin

        public WebService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> BarkodListesiGonder(List<BarkodModel> barkodlar, string listeAdi)
        {
            try
            {
                var httpClient = new HttpClient();
                var jsonData = JsonConvert.SerializeObject(new { ListeAdi = listeAdi, Barkodlar = barkodlar });
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://senin-webservisin.com/api/barkodgonder", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return false;
            }
        }
    }
}
