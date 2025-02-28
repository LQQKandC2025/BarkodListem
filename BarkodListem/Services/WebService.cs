using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BarkodListem.Data;
using System;
using BarkodListem.Models;

namespace BarkodListem.Services
{
    public class WebService
    {
        private readonly DatabaseService _databaseService;

        public WebService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }


        public async Task<bool> BarkodListesiGonder(List<BarkodModel> barkodlar, string listeAdi)
        {
            try
            {
                // 📌 Ayarlar veritabanından alınsın
                var ayarlar = await _databaseService.AyarlarGetir();

                if (ayarlar == null || string.IsNullOrEmpty(ayarlar.WebServisURL))
                {
                    Console.WriteLine("Hata: Web Servis URL'si ayarlardan alınamadı.");
                    return false;
                }

                // 📌 Web servis adresini ayarlardan al
                string webServiceUrl = $"{ayarlar.WebServisURL}:{ayarlar.Port}/api/BarkodEkle";

                // 📌 Kullanıcı bilgilerini de ekleyelim
                var jsonData = JsonConvert.SerializeObject(new
                {
                    KullaniciAdi = ayarlar.KullaniciAdi,
                    Sifre = ayarlar.Sifre,
                    ListeAdi = listeAdi,
                    Barkodlar = barkodlar
                });

                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(webServiceUrl, content);

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
