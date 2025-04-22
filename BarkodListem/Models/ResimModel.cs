using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarkodListem.Models
{
    public class ResimModel
    {
        public int id { get; set; }
        public string RESIM_SAHIP_ID { get; set; }
        public int? SIRALAMA { get; set; }
        public string RESIM_TURU { get; set; } = "NORMAL";
        public string RESIM_SAHIP { get; set; }
        public string RESIM_ADI { get; set; }
        public string RESIM_ACIKLAMA { get; set; } = string.Empty;
        public string SEVKIYAT_NO { get; set; }
    }

}
