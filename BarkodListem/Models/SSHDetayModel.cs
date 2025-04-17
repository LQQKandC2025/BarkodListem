namespace BarkodListem.Models;

public class SSHDetayModel
{
    public int SSH_DETAY_ID { get; set; }
    public int SSH_ID { get; set; }
    public int SIPARIS_ID { get; set; }
    public int SIPARIS_DETAY_ID { get; set; }
    public string URUN_KODU { get; set; }
    public decimal MIKTAR { get; set; }
    public string SORUN_ACIKLAMA { get; set; }
    public string RESIM_PATH { get; set; }
    // Diðer gerekli alanlar...
}