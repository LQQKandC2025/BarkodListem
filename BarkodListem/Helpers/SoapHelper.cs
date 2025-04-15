using System.Data;
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

            // 👇 Dataset içinde DocumentElement'e karşılık gelecek şekilde isim ayarla
            DataSet ds = new DataSet();
            ds.ReadXml(new StringReader(diffgramNode.InnerXml));

            if (ds.Tables.Count > 0)
                return ds.Tables[0];

            return new DataTable();
        }


    }
}
