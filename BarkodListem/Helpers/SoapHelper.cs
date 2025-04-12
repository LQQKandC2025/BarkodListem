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

            // resultTag örneği: "SevkiyatSorgulaResult"
            XmlNode resultNode = doc.GetElementsByTagName(resultTag).Cast<XmlNode>().FirstOrDefault();
            if (resultNode == null)
                throw new Exception("SOAP yanıtında beklenen sonuç etiketi bulunamadı: " + resultTag);

            DataSet ds = new DataSet();
            ds.ReadXml(new StringReader(resultNode.InnerXml));
            if (ds.Tables.Count > 0)
                return ds.Tables[0];

            return new DataTable();
        }
    }
}
