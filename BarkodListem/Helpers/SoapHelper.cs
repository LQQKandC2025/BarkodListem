using System.Data;
using System.Text.Json;
using System.Xml;
namespace BarkodListem.Helpers
{
    public static class SoapHelper
    {
        private static string ResultText(string soapXml, string resultTag)
        {
            var doc = new XmlDocument();
            doc.LoadXml(soapXml);
            var resultNode = doc.GetElementsByTagName(resultTag)
                                .Cast<XmlNode>()
                                .FirstOrDefault();
            if (resultNode == null)
                throw new InvalidOperationException("SOAP yanıtında beklenen sonuç etiketi bulunamadı: " + resultTag);
            return resultNode.InnerText?.Trim() ?? string.Empty;
        }

        public static DataTable ParseDataTableFromXml(string soapXml, string resultTag)
        {
            if (string.IsNullOrWhiteSpace(soapXml))
                throw new InvalidOperationException("Web servisi boş cevap döndürdü.");

            var doc = new XmlDocument();
            doc.LoadXml(soapXml);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            nsmgr.AddNamespace("diffgr", "urn:schemas-microsoft-com:xml-diffgram-v1");
            XmlNode resultNode = doc.GetElementsByTagName(resultTag).Cast<XmlNode>().FirstOrDefault();
            if (resultNode == null)
                throw new Exception("SOAP yanıtında beklenen sonuç etiketi bulunamadı: " + resultTag);
            XmlNode diffgramNode = resultNode.SelectSingleNode("diffgr:diffgram", nsmgr);
            if (diffgramNode == null)
                throw new Exception("SOAP yanıtında diffgram etiketi bulunamadı.");

            // Kayıt bulunamadığında ASMX servisi boş, kendiliğinden kapanan bir
            // diffgram döndürür. Boş metni DataSet.ReadXml'e vermek Android'de
            // Xml_MissingRoot hatasına yol açar; bu normal bir "0 kayıt" sonucudur.
            if (string.IsNullOrWhiteSpace(diffgramNode.InnerXml))
                return new DataTable();

            DataSet ds = new DataSet();
            ds.ReadXml(new StringReader(diffgramNode.InnerXml));
            if (ds.Tables.Count > 0)
                return ds.Tables[0];
            return new DataTable();
        }
        public static string soap_mesaj(string soapXml, string resultTag)
        {
            try
            {
                var resultText = ResultText(soapXml, resultTag);
                if (string.IsNullOrEmpty(resultText))
                    return "Servis boş cevap verdi.";

                try
                {
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(resultText);
                    if (dict != null && dict.TryGetValue("mesaj", out var mesaj))
                        return mesaj;
                }
                catch (JsonException)
                {
                    // Bazı servis metotları JSON yerine doğrudan metin döndürür.
                }

                return resultText;
            }
            catch (Exception ex)
            {
                return $"Hata: {ex.Message}";
            }
        }

        public static bool soap_json_basarili(string soapXml, string resultTag, out string mesaj)
        {
            try
            {
                var resultText = ResultText(soapXml, resultTag);
                using var json = JsonDocument.Parse(resultText);
                var root = json.RootElement;
                mesaj = root.TryGetProperty("mesaj", out var mesajAlani)
                    ? mesajAlani.GetString() ?? string.Empty
                    : string.Empty;
                return root.TryGetProperty("durum", out var durumAlani) &&
                       string.Equals(durumAlani.GetString(), "basarili", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                mesaj = "Servis cevabı doğrulanamadı: " + ex.Message;
                return false;
            }
        }
    }
}
