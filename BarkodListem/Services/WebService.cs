using BarkodListem.Data;
using BarkodListem.Helpers;
using BarkodListem.Models;
using System.Data;
using System.Text;

namespace BarkodListem.Services
{
    public class WebService
    {
        private readonly DatabaseService _databaseService;

        public WebService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
        private string BuildServiceUrl(string baseUrl, int port)
        {
            try
            {
                var uri = new Uri(baseUrl);
                string domain = uri.Host;
                int finalPort = uri.IsDefaultPort ? port : uri.Port;
                return $"http://{domain}:{finalPort}{uri.AbsolutePath}/BarkodService.asmx";
            }
            catch
            {
                return $"http://{baseUrl}:{port}/BarkodService.asmx";
            }
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
        public async Task<DataTable> SevkiyatSorgula(string sevkiyatNo)
        {
            var ayarlar = await _databaseService.AyarlarGetir();

            if (ayarlar == null || string.IsNullOrEmpty(ayarlar.WebServisURL))
                throw new Exception("Web Servis URL'si ayarlardan alınamadı.");

            // 📌 Portlu URL oluştur
            Uri baseUri = new Uri(ayarlar.WebServisURL);
            string domain = baseUri.Host;
            int port = baseUri.IsDefaultPort ? ayarlar.Port : baseUri.Port;
            string webServiceUrl = $"http://{domain}:{port}{baseUri.AbsolutePath}/BarkodService.asmx";

            // 📌 SOAP Request
            string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
               xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
               xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <SevkiyatSorgula xmlns=""http://barkodwebservice.com/"">
      <username>{ayarlar.KullaniciAdi}</username>
      <password>{ayarlar.Sifre}</password>
      <sevkiyatNo>{sevkiyatNo}</sevkiyatNo>
    </SevkiyatSorgula>
  </soap:Body>
</soap:Envelope>";

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(60);

                var request = new HttpRequestMessage(HttpMethod.Post, webServiceUrl);
                request.Headers.Add("SOAPAction", "http://barkodwebservice.com/SevkiyatSorgula");
                request.Content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");

                var response = await client.SendAsync(request);
                string resultXml = await response.Content.ReadAsStringAsync();

                // 🔍 Yalnızca <SevkiyatSorgulaResult> içeriğini ayıkla
                string cleanedXml = ExtractDataTableXml(resultXml, "SevkiyatSorgulaResult");

                // 📥 XML'i DataTable'a dönüştür
                var ds = new DataSet();
                ds.ReadXml(new StringReader(cleanedXml));
                return ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
            }
        }


        public async Task<string> SevkiyatGuncelle(string sevkiyatNo, string teslimNotu, string adiSoyadi, string tel1, string onayKodu)
        {
            var ayarlar = await _databaseService.AyarlarGetir();

            Uri baseUri = new Uri(ayarlar.WebServisURL);
            string domain = baseUri.Host;
            int port = baseUri.IsDefaultPort ? ayarlar.Port : baseUri.Port;

            string webServiceUrl = $"http://{domain}:{port}{baseUri.AbsolutePath}/BarkodService.asmx";

            string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
               xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
               xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <SevkiyatGuncelle xmlns=""http://barkodwebservice.com/"">
      <username>{ayarlar.KullaniciAdi}</username>
      <password>{ayarlar.Sifre}</password>
      <sevkiyatNo>{sevkiyatNo}</sevkiyatNo>
      <teslimNotu>{teslimNotu}</teslimNotu>
      <adiSoyadi>{adiSoyadi}</adiSoyadi>
      <tel1>{tel1}</tel1>
      <teslimOnayKodu>{onayKodu}</teslimOnayKodu>
    </SevkiyatGuncelle>
  </soap:Body>
</soap:Envelope>";

            using (var client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, webServiceUrl);
                request.Headers.Add("SOAPAction", "http://barkodwebservice.com/SevkiyatGuncelle");
                request.Content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");

                var response = await client.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
        }
        private string ExtractDataTableXml(string soapXml, string resultTag)
        {
            var startTag = $"<{resultTag}>";
            var endTag = $"</{resultTag}>";

            int start = soapXml.IndexOf(startTag) + startTag.Length;
            int end = soapXml.IndexOf(endTag);

            if (start <= 0 || end <= 0 || end <= start)
                throw new Exception("SOAP içeriğinde beklenen veri bulunamadı.");

            string innerXml = soapXml.Substring(start, end - start);
            return $"<Root>{innerXml}</Root>"; // Tek kök elementli XML haline getir
        }

        public async Task<DataTable> SevkiyatUrunListesi(string sevkiyatNo)
        {
            var ayarlar = await _databaseService.AyarlarGetir();

            // 📌 SOAP request şablonu
            string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
               xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
               xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <SevkiyatUrunListesi xmlns=""http://barkodwebservice.com/"">
      <username>{ayarlar.KullaniciAdi}</username>
      <password>{ayarlar.Sifre}</password>
      <sevkiyatNo>{sevkiyatNo}</sevkiyatNo>
    </SevkiyatUrunListesi>
  </soap:Body>
</soap:Envelope>";

            string url = BuildServiceUrl(ayarlar.WebServisURL, ayarlar.Port);

            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("SOAPAction", "http://barkodwebservice.com/SevkiyatUrunListesi");
            request.Content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");

            var response = await client.SendAsync(request);
            var resultXml = await response.Content.ReadAsStringAsync();

            return SoapHelper.ParseDataTableFromXml(resultXml, "SevkiyatUrunListesiResult");
        }



    }
}
