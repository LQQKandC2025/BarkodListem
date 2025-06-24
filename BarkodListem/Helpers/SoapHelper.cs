using System.Data;
using System.Text.Json;
using System.Xml;
namespace BarkodListem.Helpers
{
    public static class SoapHelper
    {
        public static DataTable ParseDataTableFromXml(string soapXml, string resultTag)
        {
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
                string startTag = $"<{resultTag}>";
                string endTag = $"</{resultTag}>";
                int start = soapXml.IndexOf(startTag) + startTag.Length;
                int end = soapXml.IndexOf(endTag);
                if (start < 0 || end < 0 || end <= start)
                    return "Cevap ayrıştırılamadı.";
                string json = soapXml.Substring(start, end - start);
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return dict != null && dict.ContainsKey("mesaj") ? dict["mesaj"] : "Mesaj alanı bulunamadı.";
            }
            catch (Exception ex)
            {
                return $"Hata: {ex.Message}";
            }
        }
    }
}
