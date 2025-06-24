using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace BarkodListem.Models
{
    public class UrunModel
    {
        public string STOK_ADI { get; set; }
        public string STOK_SPEC { get; set; }
        public decimal MIKTAR { get; set; }
        public string SEVK_ARAC_ID { get; set; }
        public string SIP_STR_ID { get; set; }
        public string SEVK_FIS_ID { get; set; }
        public string STOK_ID { get; set; }
        public string SUBE_KODU { get; set; }
        public string CARI_ID { get; set; }
        public string SEVKIYAT_NO { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public int Sirano { get; set; }
        private bool _sorunVar;
        public bool SORUNVAR
        {
            get => _sorunVar;
            set
            {
                if (_sorunVar != value)
                {
                    _sorunVar = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ResimKlasoru => SEVKIYAT_NO;
        public string DosyaKodu => $"{SEVK_FIS_ID}_{SIP_STR_ID}";
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}