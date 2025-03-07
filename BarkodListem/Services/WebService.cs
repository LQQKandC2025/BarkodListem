using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BarkodListem.Data;
using System;
using BarkodListem.Models;
using Java.Net;

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

                // 📌 Kullanıcının girdiği URL'yi URI formatına çevir
                Uri baseUri = new Uri(ayarlar.WebServisURL);

                // 📌 Alan adını ve portu çek
                string domain = baseUri.Host;
                int port = baseUri.IsDefaultPort ? ayarlar.Port : baseUri.Port;

                // 📌 Son kısmı ekleyerek tam web servisi adresini oluştur
                string webServiceUrl = $"http://{domain}:{port}{baseUri.AbsolutePath}/BarkodService.asmx";

                // 📌 Barkodları XML formatına çevirme
                StringBuilder barkodXml = new StringBuilder();
                foreach (var barkod in barkodlar)
                {
                    barkodXml.Append($"<string>{barkod.Barkod}</string>");
                }

                // 📌 SOAP isteğini oluştur
                string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
        <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
                       xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
                       xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
          <soap:Body>
            <BarkodEkle xmlns=""http://barkodwebservice.com/"">
              <username>{ayarlar.KullaniciAdi}</username>
              <password>{ayarlar.Sifre}</password>
              <listeAdi>{listeAdi}</listeAdi>
              <barkodlar>{barkodXml}</barkodlar>
            </BarkodEkle>
          </soap:Body>
        </soap:Envelope>";

                // 📌 HTTP isteğini oluştur
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(2);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, webServiceUrl);
                    request.Headers.Add("SOAPAction", "http://barkodwebservice.com/BarkodEkle");
                    request.Content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");

                    // 📌 İstek gönder
                    HttpResponseMessage response = await client.SendAsync(request);

                    // 📌 Yanıtı kontrol et
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"📌 SOAP Yanıtı: {responseContent}");

                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return false;
            }
        }


    }
}
