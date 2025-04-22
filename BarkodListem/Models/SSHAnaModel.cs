using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
namespace BarkodListem.Models
{
    [Table("SSH_ANA")]
    public class SSHAnaModel
    {
        public int? SSH_ID { get; set; } // boş geçilecek
        public DateTime TARIH { get; set; }
        public string SUBE_KODU { get; set; }
        public string EVRAK_NO { get; set; } = string.Empty;
        public int? CARI_ID { get; set; } = null;
        public string ACIKLAMA { get; set; }
        public string DURUMU { get; set; } = "BEKLEMEDE";
        public string SEVKIYAT_NO { get; set; }
    }

}
