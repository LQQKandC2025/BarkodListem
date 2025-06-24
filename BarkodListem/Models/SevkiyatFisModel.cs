using SQLite;
[Table("SEVKIYAT_FISLER")]
public class SevkiyatFisModel
{
    [PrimaryKey]
    public DateTime? SEVK_TARIH { get; set; }
    public string SEVKIYAT_NO { get; set; }
    public string ISLEM_TURU { get; set; }
    public string TESLIM_NOTU { get; set; }
    public string ADI_SOYADI { get; set; }
    public string TEL_1 { get; set; }
    public string TESLIM_ONAY_KODU { get; set; }
    public string ARAC_PLAKA { get; set; }
    public string MOBIL_PERSONEL { get; set; }
}
