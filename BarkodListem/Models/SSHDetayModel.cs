using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
namespace BarkodListem.Models
{
    [Table("SSH_DETAY")]
    public class SSHDetayModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } // 
        public int? SSH_DETAY_ID { get; set; }
        public int? SIRANO { get; set; }
        public DateTime TARIH { get; set; }
        public string SSH_NO { get; set; } = string.Empty;
        public string SSH_TURU { get; set; } = "ÜCRETSİZ";
        public string SORUN_DURUM { get; set; } = "İŞLEMDE";
        public string SORUN_ACIKLAMA { get; set; }
        public double MIKTAR { get; set; }
        public DateTime SSH_SURE { get; set; } = new DateTime(2010, 1, 1, 0, 10, 0);
        public double TUTAR { get; set; } = 0;
        public int? SSH_ID { get; set; }
        public int STOK_ID { get; set; }
        public int SPEC_ID { get; set; }
        public int SEVK_FIS_ID { get; set; }
        public int SSH_SEVK_FIS_ID { get; set; } = 0;
        public int IRS_STR_ID { get; set; } = 0;
        public string SORUN_KAYNAGI { get; set; } = string.Empty;
        public string SORUN { get; set; } = string.Empty;
        public string SORUN_DETAY { get; set; } = string.Empty;
        public string SEVKIYAT_NO { get; set; } = string.Empty;
    }

}
