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
                var ayarlar = await _databaseService.AyarlarGetir();
                if (ayarlar == null || string.IsNullOrEmpty(ayarlar.WebServisURL))
                {
                    Console.WriteLine("Hata: Web Servis URL'si ayarlardan alınamadı.");
                    return false;
                }
                Uri baseUri = new Uri(ayarlar.WebServisURL);
                string domain = baseUri.Host;
                int port = baseUri.IsDefaultPort ? ayarlar.Port : baseUri.Port;
                string webServiceUrl = $"http://{domain}:{port}{baseUri.AbsolutePath}/BarkodService.asmx";
                StringBuilder barkodXml = new StringBuilder();
                foreach (var barkod in barkodlar)
                {
                    barkodXml.Append($"<string>{barkod.Barkod}</string>");
                }
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
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(2);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, webServiceUrl);
                    request.Headers.Add("SOAPAction", "http://barkodwebservice.com/BarkodEkle");
                    request.Content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
                    HttpResponseMessage response = await client.SendAsync(request);
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
            Uri baseUri = new Uri(ayarlar.WebServisURL);
            string domain = baseUri.Host;
            int port = baseUri.IsDefaultPort ? ayarlar.Port : baseUri.Port;
            string webServiceUrl = $"http://{domain}:{port}{baseUri.AbsolutePath}/BarkodService.asmx";
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
                string cleanedXml = ExtractDataTableXml(resultXml, "SevkiyatSorgulaResult");
                var ds = new DataSet();
                ds.ReadXml(new StringReader(cleanedXml));
                return ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
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
        public async Task<string> SevkiyatKaydet(string username, string password, SevkiyatFisModel model)
        {
            string url = BuildServiceUrl((await _databaseService.AyarlarGetir()).WebServisURL, (await _databaseService.AyarlarGetir()).Port);
            var ayarlar = await _databaseService.AyarlarGetir();
            string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
               xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
               xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <SevkiyatKaydet xmlns=""http://barkodwebservice.com/"">
      <username>{ayarlar.KullaniciAdi}</username>
      <password>{ayarlar.Sifre}</password>
      <sevkiyat>
        <SEVK_TARIH>{model.SEVK_TARIH?.ToString("yyyy-MM-ddTHH:mm:ss")}</SEVK_TARIH>
        <SEVKIYAT_NO>{model.SEVKIYAT_NO}</SEVKIYAT_NO>
        <ISLEM_TURU>{model.ISLEM_TURU}</ISLEM_TURU>
        <TESLIM_NOTU>{model.TESLIM_NOTU}</TESLIM_NOTU>
        <ADI_SOYADI>{model.ADI_SOYADI}</ADI_SOYADI>
        <TEL_1>{model.TEL_1}</TEL_1>
        <TESLIM_ONAY_KODU>{model.TESLIM_ONAY_KODU}</TESLIM_ONAY_KODU>
        <ARAC_PLAKA>{model.ARAC_PLAKA}</ARAC_PLAKA>
        <MOBIL_PERSONEL>{model.MOBIL_PERSONEL}</MOBIL_PERSONEL>
      </sevkiyat>
    </SevkiyatKaydet>
  </soap:Body>
</soap:Envelope>";
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("SOAPAction", "http://barkodwebservice.com/SevkiyatKaydet");
            request.Content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            var response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> SSHKaydet(string username, string password, SSHAnaModel ana, List<SSHDetayModel> detaylar)
        {
            string url = BuildServiceUrl((await _databaseService.AyarlarGetir()).WebServisURL, (await _databaseService.AyarlarGetir()).Port);
            var ayarlar = await _databaseService.AyarlarGetir();
            StringBuilder detayXml = new StringBuilder();
            foreach (var d in detaylar)
            {
                detayXml.Append($"<SSHDetayModel>" +
                    $"<SIRANO>{d.SIRANO}</SIRANO>" +
                    $"<STOK_ID>{d.STOK_ID}</STOK_ID>" +
                    $"<SPEC_ID>{d.SPEC_ID}</SPEC_ID>" +
                    $"<MIKTAR>{d.MIKTAR}</MIKTAR>" +
                    $"<SSH_TURU>{d.SSH_TURU}</SSH_TURU>" +
                    $"<SORUN_KAYNAGI>{d.SORUN_KAYNAGI}</SORUN_KAYNAGI>" +
                    $"<SORUN_DURUM>{d.SORUN_DURUM}</SORUN_DURUM>" +
                    $"<TUTAR>{d.TUTAR}</TUTAR>" +
                    $"<SSH_SURE>{d.SSH_SURE:yyyy-MM-ddTHH:mm:ss}</SSH_SURE>" +
                    $"<SORUN>{d.SORUN}</SORUN>" +
                    $"<SORUN_DETAY>{d.SORUN_DETAY}</SORUN_DETAY>" +
                    $"<SORUN_ACIKLAMA>{d.SORUN_ACIKLAMA}</SORUN_ACIKLAMA>" +
                    $"<SEVK_FIS_ID>{d.SEVK_FIS_ID}</SEVK_FIS_ID>" +
                    $"<IRS_STR_ID>{d.IRS_STR_ID}</IRS_STR_ID>" +
                    $"<SEVKIYAT_NO>{d.SEVKIYAT_NO}</SEVKIYAT_NO>"+
                    $"</SSHDetayModel>");
            }
            string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
               xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
               xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <SSHKaydet xmlns=""http://barkodwebservice.com/"">
      <username>{ayarlar.KullaniciAdi}</username>
      <password>{ayarlar.Sifre}</password>
      <ana>
        <SEVKIYAT_NO>{ana.SEVKIYAT_NO}</SEVKIYAT_NO>
        <SUBE_KODU>{ana.SUBE_KODU}</SUBE_KODU>
       <CARI_ID>{(ana.CARI_ID > 0 ? ana.CARI_ID.ToString() : "0")}</CARI_ID>
        <TARIH>{ana.TARIH.ToString("yyyy-MM-ddTHH:mm:ss")}</TARIH>
        <EVRAK_NO>0</EVRAK_NO>
        <DURUMU>{ana.DURUMU}</DURUMU>
      </ana>
      <detaylar>
        {detayXml}
      </detaylar>
    </SSHKaydet>
  </soap:Body>
</soap:Envelope>";
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("SOAPAction", "http://barkodwebservice.com/SSHKaydet");
            request.Content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            var response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> ResimleriKaydet(string username, string password, List<ResimModel> resimler)
        {
            string url = BuildServiceUrl((await _databaseService.AyarlarGetir()).WebServisURL, (await _databaseService.AyarlarGetir()).Port);
            var ayarlar = await _databaseService.AyarlarGetir();
            StringBuilder xml = new StringBuilder();
            foreach (var r in resimler)
            {
                xml.Append($"<ResimModel>" +
    $"<SEVKIYAT_NO>{r.SEVKIYAT_NO}</SEVKIYAT_NO>" +
    $"<STOK_ID>{r.STOK_ID}</STOK_ID>" +
    $"<SIPSTR_ID>{r.SIPSTR_ID}</SIPSTR_ID>" +
    $"<DOSYA_ADI>{r.DOSYA_ADI}</DOSYA_ADI>" +
    $"<YOL>{r.YOL}</YOL>" +
    $"<TARIH>{(r.TARIH.HasValue ? r.TARIH.Value.ToString("yyyy-MM-ddTHH:mm:ss") : "")}</TARIH>" +
    $"<TIPI>{r.TIPI}</TIPI>" +
    $"</ResimModel>");
            }
            string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
               xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
               xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <ResimleriKaydet xmlns=""http://barkodwebservice.com/"">
      <username>{ayarlar.KullaniciAdi}</username>
      <password>{ayarlar.Sifre}</password>
      <resimler>
        {xml}
      </resimler>
    </ResimleriKaydet>
  </soap:Body>
</soap:Envelope>";
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("SOAPAction", "http://barkodwebservice.com/ResimleriKaydet");
            request.Content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            var response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> ResimYukle(string username, string password, string dosyaAdi, byte[] resimData)
        {
            var ayarlar = await _databaseService.AyarlarGetir();
            string url = BuildServiceUrl(ayarlar.WebServisURL, ayarlar.Port);
            string soapBody = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
               xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
               xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <ResimYukle xmlns=""http://barkodwebservice.com/"">
      <username>{username}</username>
      <password>{password}</password>
      <dosyaAdi>{dosyaAdi}</dosyaAdi>
      <resimData>{Convert.ToBase64String(resimData)}</resimData>
    </ResimYukle>
  </soap:Body>
</soap:Envelope>";
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("SOAPAction", "http://barkodwebservice.com/ResimYukle");
            request.Content = new StringContent(soapBody, Encoding.UTF8, "text/xml");
            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }
}
