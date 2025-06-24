using BarkodListem.Data;
using BarkodListem.Helpers;
using BarkodListem.Models;
using BarkodListem.Popups;
using BarkodListem.Services;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using SQLitePCL;
using System.ComponentModel;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;
namespace BarkodListem.ViewModels
{
    public class SevkiyatDetayViewModel : INotifyPropertyChanged
    {
        private readonly WebService _webService;
        private readonly string sevkiyatNo;
        public string dbPath = Path.Combine(FileSystem.AppDataDirectory, "barkodlistem.db");
        public DatabaseService _databaseService;
        public event PropertyChangedEventHandler PropertyChanged;
        public string SEVK_FIS_ID { get; set; }
        public string SEVK_TARIH { get; set; }
        public string SEVKIYAT_NO { get; set; }
        public string ISLEM_TURU { get; set; }
        public string ADI_SOYADI { get; set; }
        public string TESLIM_NOTU { get; set; }
        public string TESLIM_ALAN { get; set; }
        public string TEL1 { get; set; }
        public string TESLIM_ONAY_KODU { get; set; }
        public string ARAC_PLAKA { get; set; }
        public string MOBIL_PERSONEL { get; set; }
        public ICommand KaydetCommand => new Command(async () => await KaydetAsync());
        public ICommand UrunListesiCommand => new Command(OnUrunListesi);
        public ICommand KodGonderCommand => new Command(OnKodGonder);
        public DataTable dt { get; set; } = new DataTable();

        public string sonuc1 { get; set; }=string.Empty;
        public SevkiyatDetayViewModel(string sevkiyatNo)
        {
            AppGlobals.AktifSevkiyat = sevkiyatNo;
            this.sevkiyatNo = sevkiyatNo;
            var databaseService = new DatabaseService(dbPath);
            _webService = new WebService(databaseService);
        }
        public async Task LoadSevkiyatAsync()
        {
            try
            {
                dt = await _webService.SevkiyatSorgula(sevkiyatNo);
                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    string? ADI_SOYADI = row["ADI_SOYADI"]?.ToString();
                    string? SEVK_TARIH = row["SEVK_TARIH"]?.ToString();
                    string? SEVKIYAT_NO = row["SEVKIYAT_NO"]?.ToString();
                    string? ISLEM_TURU = row["ISLEM_TURU"]?.ToString();
                    string? TESLIM_NOTU = row["TESLIM_NOTU"]?.ToString();
                    string? TESLIM_ALAN = row["ADI_SOYADI"]?.ToString();
                    string? TEL1 = row["TEL_1"]?.ToString();
                    string? TESLIM_ONAY_KODU = row["TESLIM_ONAY_KODU"]?.ToString();
                    string? ARAC_PLAKA = row["ARAC_PLAKA"]?.ToString();
                    string? MOBIL_PERSONEL = AppGlobals.mobil_id.ToString();
                    NotifyAll();
                } else
                {
                    await Application.Current.MainPage.DisplayAlert("Hata",sevkiyatNo + " sevkiyat kodu ile ilgili bir kayıt bulunamadı.", "Tamam");
                }
                
            }
            catch (Exception ex)
            {

                await Application.Current.MainPage.DisplayAlert(
                "Hata",
                "Bağlantı kurulamadı, servis ayarlarınızı kontrol edin.\n\nHata mesajı:\n" + ex.Message,
                "Tamam");
            }
        }
        private async Task KaydetAsync()
        {
            
            var progressPopup = new ProgressPopup();
            Application.Current.MainPage.ShowPopup(progressPopup);
            _databaseService = new DatabaseService(dbPath);
            var ayarlar = await _databaseService.AyarlarGetir();
            try
            {
                progressPopup.AddStep("Sevkiyat kaydediliyor...");
                var db = await DatabaseService.GetConnectionAsync();
                string aktifNo = SEVKIYAT_NO;
                var sevkiyat = await db.Table<SevkiyatFisModel>()
                                      .Where(x => x.SEVKIYAT_NO == aktifNo)
                                      .FirstOrDefaultAsync();
                if (sevkiyat == null)
                {
                    var yeniSevkiyat = new SevkiyatFisModel
                    {
                        SEVKIYAT_NO = SEVKIYAT_NO,
                        SEVK_TARIH = DateTime.Now,
                        ISLEM_TURU = ISLEM_TURU,
                        TESLIM_NOTU = TESLIM_NOTU,
                        ADI_SOYADI = ADI_SOYADI,
                        TEL_1 = TEL1,
                        TESLIM_ONAY_KODU = TESLIM_ONAY_KODU,
                        ARAC_PLAKA = ARAC_PLAKA,
                        MOBIL_PERSONEL = ayarlar.user_id.ToString()
                    };
                    await db.InsertAsync(yeniSevkiyat);
                    sonuc1 = await _webService.SevkiyatKaydet(ayarlar.KullaniciAdi, ayarlar.Sifre, yeniSevkiyat);
                }
                else
                {
                    sevkiyat.SEVKIYAT_NO = SEVKIYAT_NO;
                    sevkiyat.SEVK_TARIH = DateTime.Now;
                    sevkiyat.ISLEM_TURU = ISLEM_TURU;
                    sevkiyat.TESLIM_NOTU =TESLIM_NOTU;
                    sevkiyat.ADI_SOYADI = ADI_SOYADI;
                    sevkiyat.TEL_1 = TEL1;
                    sevkiyat.TESLIM_ONAY_KODU = TESLIM_ONAY_KODU;
                    sevkiyat.ARAC_PLAKA = ARAC_PLAKA;
                    await db.UpdateAsync(sevkiyat);
                    sonuc1 = await _webService.SevkiyatKaydet(ayarlar.KullaniciAdi, ayarlar.Sifre, sevkiyat);
                }
                
                sonuc1=SoapHelper.soap_mesaj(sonuc1, "SevkiyatKaydetResult");
                progressPopup.CompleteStep("Sevkiyat kaydediliyor...");
                progressPopup.AddStep("SSH kaydediliyor...");
                var sshAna = await db.Table<SSHAnaModel>()
                                    .Where(x => x.SEVKIYAT_NO == aktifNo)
                                    .FirstOrDefaultAsync();
                var sshDetaylar = await db.Table<SSHDetayModel>()
                                         .Where(x => x.SEVKIYAT_NO == aktifNo)
                                         .ToListAsync();
                if (sshAna != null || sshDetaylar.Any())
                {
                    var sonuc2 = await _webService.SSHKaydet(ayarlar.KullaniciAdi, ayarlar.Sifre, sshAna, sshDetaylar);
                    sonuc2= SoapHelper.soap_mesaj(sonuc2, "SSHKaydetResult");
                }
                progressPopup.CompleteStep("SSH kaydediliyor...");
                progressPopup.AddStep("Resimler tablosu kaydediliyor...");
                var resimler = await db.Table<ResimModel>()
                                      .Where(x => x.SEVKIYAT_NO == aktifNo)
                                      .ToListAsync();
                if (resimler.Any())
                {
                    var sonuc3 = await _webService.ResimleriKaydet(ayarlar.KullaniciAdi, ayarlar.Sifre, resimler);
                    sonuc3= SoapHelper.soap_mesaj(sonuc3, "ResimleriKaydetResult");
                }
                progressPopup.CompleteStep("Resimler tablosu kaydediliyor...");
                progressPopup.AddStep("Fotoğraflar gönderiliyor...");
                var gonderilecekResimler = await db.Table<ResimModel>()
                  .Where(x => x.SEVKIYAT_NO == AppGlobals.AktifSevkiyat && x.GONDERIM == 0)
                  .ToListAsync();
                int gonderilen = 0;
                int atlanan = 0;
                foreach (var r in gonderilecekResimler)
                {
                    try
                    {
                        var klasorYolu = Path.Combine(FileSystem.AppDataDirectory, "Resimler", AppGlobals.AktifSevkiyat);
                        var tamYol = Path.Combine(klasorYolu, r.DOSYA_ADI);
                        if (!File.Exists(tamYol))
                        {
                            Console.WriteLine($"❌ Dosya bulunamadı: {tamYol}");
                            atlanan++;
                            continue;
                        }
                        byte[] resimBytes = File.ReadAllBytes(tamYol);
                        var sonuc = await _webService.ResimYukle(ayarlar.KullaniciAdi, ayarlar.Sifre, r.DOSYA_ADI, resimBytes);
                        if (sonuc.Contains("\"durum\":\"basarili\""))
                        {
                            r.GONDERIM = 1;
                            await db.UpdateAsync(r);
                            gonderilen++;
                        }
                        else
                        {
                            Console.WriteLine($"❌ Gönderim hatası: {r.DOSYA_ADI} => {sonuc}");
                            atlanan++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ HATA: {r.DOSYA_ADI} => {ex.Message}");
                        atlanan++;
                    }
                }
                progressPopup.CompleteStep("Fotoğraflar gönderiliyor...");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", "Bir hata oluştu.\n\n Hata Mesajı:\n" + ex.Message, "Tamam");
            }
            sonuc1=string.Empty;
        }
        private void OnUrunListesi()
        {
            Application.Current.MainPage.DisplayAlert("Bilgi", "Ürün listesi butonuna basıldı.", "Tamam");
        }
        private void OnKodGonder()
        {
            Application.Current.MainPage.DisplayAlert("Bilgi", "Kod gönderme işlemi burada yapılacak.", "Tamam");
        }
        private void NotifyAll()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
