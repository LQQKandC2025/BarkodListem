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

        // TF veya SSH çekilen resim için kullanýlacak
        public string ResimKlasoru => SEVK_FIS_ID;
        public string DosyaKodu => $"{SEVK_FIS_ID}_{SIP_STR_ID}";
    }
}
