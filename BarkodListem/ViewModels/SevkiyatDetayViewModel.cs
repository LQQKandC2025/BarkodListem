using System;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Input;
using BarkodListem.Services;
using BarkodListem.Data;
using Microsoft.Maui.Controls;

namespace BarkodListem.ViewModels
{
    public class SevkiyatDetayViewModel : INotifyPropertyChanged
    {
        private readonly WebService _webService;
        private readonly string sevkiyatNo;
        public string dbPath = Path.Combine(FileSystem.AppDataDirectory, "barkodlistem.db");
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
            try
            {
                var db = await DatabaseService.GetConnectionAsync();

                var model = new SevkiyatFisModel
                {
                    SEVK_FIS_ID = int.TryParse(SEVK_FIS_ID, out int fisId) ? fisId : 0,
                    SEVK_TARIH = DateTime.TryParse(SEVK_TARIH, out var tarih) ? tarih : null,
                    SEVKIYAT_NO = SEVKIYAT_NO,
                    ISLEM_TURU = ISLEM_TURU,
                    TESLIM_NOTU = TESLIM_NOTU,
                    ADI_SOYADI = TESLIM_ALAN,
                    TEL_1 = TEL1,
                    TESLIM_ONAY_KODU = TESLIM_ONAY_KODU,
                    MOBIL_PERSONEL = AppGlobals.mobil_id.ToString()
                };

                // G�ncelleme varsa silip tekrar ekle
                var mevcut = await db.Table<SevkiyatFisModel>()
                                     .Where(x => x.SEVK_FIS_ID == model.SEVK_FIS_ID)
                                     .FirstOrDefaultAsync();

                if (mevcut != null)
                    await db.DeleteAsync(mevcut);

                await db.InsertAsync(model);

                await Application.Current.MainPage.DisplayAlert("Kay�t", "Sevkiyat bilgileri kaydedildi.", "Tamam");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private void OnUrunListesi()
        {
            // �r�n listesi sayfas�na y�nlendirme yap�labilir.
            Application.Current.MainPage.DisplayAlert("Bilgi", "�r�n listesi butonuna bas�ld�.", "Tamam");
        }

        private void OnKodGonder()
        {
            // Kod g�nderme i�lemi daha sonra eklenecek.
            Application.Current.MainPage.DisplayAlert("Bilgi", "Kod g�nderme i�lemi burada yap�lacak.", "Tamam");
        }

        private void NotifyAll()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
