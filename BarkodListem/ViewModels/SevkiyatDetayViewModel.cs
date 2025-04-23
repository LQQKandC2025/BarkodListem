using System;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Input;
using BarkodListem.Services;
using BarkodListem.Data;
using Microsoft.Maui.Controls;
using BarkodListem.Models;

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
        public string TESLIM_NOTU { get; set; }
        public string TESLIM_ALAN { get; set; }
        public string TEL1 { get; set; }
        public string TESLIM_ONAY_KODU { get; set; }
        public string ARAC_PLAKA { get; set; }

        public ICommand KaydetCommand => new Command(async () => await KaydetAsync());
        public ICommand UrunListesiCommand => new Command(OnUrunListesi);
        public ICommand KodGonderCommand => new Command(OnKodGonder);
  

        public SevkiyatDetayViewModel(string sevkiyatNo)
        {
            this.sevkiyatNo = sevkiyatNo;
           
            var databaseService = new DatabaseService(dbPath);
            _webService = new WebService(databaseService);
        }

        public async Task LoadSevkiyatAsync()
        {
            try
            {
                var dt = await _webService.SevkiyatSorgula(sevkiyatNo);
                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    SEVK_FIS_ID = row["SEVK_FIS_ID"]?.ToString();
                    SEVK_TARIH = row["SEVK_TARIH"]?.ToString();
                    SEVKIYAT_NO = row["SEVKIYAT_NO"]?.ToString();
                    ISLEM_TURU = row["ISLEM_TURU"]?.ToString();
                    TESLIM_NOTU = row["TESLIM_NOTU"]?.ToString();
                    TESLIM_ALAN = row["ADI_SOYADI"]?.ToString();
                    TEL1 = row["TEL_1"]?.ToString();
                    TESLIM_ONAY_KODU = row["TESLIM_ONAY_KODU"]?.ToString();
                    ARAC_PLAKA = row["ARAC_PLAKA"]?.ToString();

                    NotifyAll();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private async Task KaydetAsync()
        {
            _databaseService = new DatabaseService(dbPath);
            var ayarlar = await _databaseService.AyarlarGetir();
            try
            {
                var db = await DatabaseService.GetConnectionAsync();
               
                string aktifNo = SEVKIYAT_NO;

                // 1️⃣ Sevkiyat fişi
                var sevkiyat = await db.Table<SevkiyatFisModel>()
                                       .Where(x => x.SEVKIYAT_NO == aktifNo)
                                       .FirstOrDefaultAsync();

                if (sevkiyat != null)
                {
                    // Web servise gönder
                    var sonuc1 = await _webService.SevkiyatKaydet(ayarlar.KullaniciAdi, ayarlar.Sifre, sevkiyat);
                    await Application.Current.MainPage.DisplayAlert("Sevkiyat", sonuc1, "Tamam");
                }

                // 2️⃣ SSH_ANA
                var sshAna = await db.Table<SSHAnaModel>()
                                     .Where(x => x.SEVKIYAT_NO == aktifNo)
                                     .FirstOrDefaultAsync();

                // 3️⃣ SSH_DETAY
                var sshDetaylar = await db.Table<SSHDetayModel>()
                                          .Where(x => x.SEVKIYAT_NO == aktifNo)
                                          .ToListAsync();

                if (sshAna != null || sshDetaylar.Any())
                {
                    var sonuc2 = await _webService.SSHKaydet(ayarlar.KullaniciAdi, ayarlar.Sifre, sshAna, sshDetaylar);
                    await Application.Current.MainPage.DisplayAlert("SSH", sonuc2, "Tamam");
                }

                // 4️⃣ RESIMLER
                var resimler = await db.Table<ResimModel>()
                                       .Where(x => x.SEVKIYAT_NO == aktifNo)
                                       .ToListAsync();

                if (resimler.Any())
                {
                    var sonuc3 = await _webService.ResimleriKaydet(ayarlar.KullaniciAdi, ayarlar.Sifre, resimler);
                    await Application.Current.MainPage.DisplayAlert("Resimler", sonuc3, "Tamam");
                }

                await Application.Current.MainPage.DisplayAlert("Kayıt", "Tüm bilgiler başarıyla gönderildi.", "Tamam");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private void OnUrunListesi()
        {
            // Ürün listesi sayfasına yönlendirme yapılabilir.
            Application.Current.MainPage.DisplayAlert("Bilgi", "Ürün listesi butonuna basıldı.", "Tamam");
        }

        private void OnKodGonder()
        {
            // Kod gönderme işlemi daha sonra eklenecek.
            Application.Current.MainPage.DisplayAlert("Bilgi", "Kod gönderme işlemi burada yapılacak.", "Tamam");
        }

        private void NotifyAll()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
