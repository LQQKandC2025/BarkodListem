using BarkodListem.Models;
using BarkodListem.Services;
using Microsoft.Maui.ApplicationModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BarkodListem.Data;
using BarkodListem.Pages;
{

}

namespace BarkodListem.ViewModels
{
    public class UrunListesiViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<UrunModel> Urunler { get; set; } = new();
        public ICommand FotografCekCommand { get; set; }
        public ICommand FotografSSHCommand { get; set; }

        private readonly WebService _webService;
        private string _sevkiyatNo;
        public string dbPath = Path.Combine(FileSystem.AppDataDirectory, "barkodlistem.db");
        public event PropertyChangedEventHandler PropertyChanged;

        public UrunListesiViewModel()
        {
            var databaseService = new DatabaseService(dbPath);
            _webService = new WebService(databaseService);

            FotografCekCommand = new Command<UrunModel>(async (urun) => await FotografCekAsync(urun, "TF"));
            FotografSSHCommand = new Command<UrunModel>(async (urun) => await FotografCekAsync(urun, "SSH"));
        }

        public async Task YukleAsync(string sevkiyatNo)
        {
            _sevkiyatNo = sevkiyatNo;
            var dt = await _webService.SevkiyatUrunListesi(sevkiyatNo);
            Urunler.Clear();
            foreach (System.Data.DataRow row in dt.Rows)
            {
                Urunler.Add(new UrunModel
                {
                    STOK_ADI = row["STOK_ADI"].ToString(),
                    STOK_SPEC = row["STOK_SPEC"].ToString(),
                    MIKTAR = Convert.ToDecimal(row["MIKTAR"]),
                    SEVK_ARAC_ID = row["SEVK_ARAC_ID"].ToString(),
                    SIP_STR_ID = row["SIP_STR_ID"].ToString(),
                    SEVK_FIS_ID = row["SEVK_FIS_ID"].ToString(),
                    STOK_ID = row["STOK_ID"].ToString(),
                });
            }
            Notify(nameof(Urunler));
        }

        private async Task FotografCekAsync(UrunModel urun, string tip)
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", "Cihaz kamera deste�i vermiyor.", "Tamam");
                return;
            }

            try
            {
                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo == null || urun == null) return;

                var saveFolder = Path.Combine(FileSystem.Current.AppDataDirectory, urun.ResimKlasoru);
                Directory.CreateDirectory(saveFolder);

                int sira = Directory.GetFiles(saveFolder, $"{tip}_{urun.DosyaKodu}_*.jpg").Length + 1;
                string fileName = $"{tip}_{urun.DosyaKodu}_{sira}.jpg";
                string fullPath = Path.Combine(saveFolder, fileName);

                using var stream = await photo.OpenReadAsync();
                using var fileStream = File.OpenWrite(fullPath);
                await stream.CopyToAsync(fileStream);

                await Application.Current.MainPage.DisplayAlert("Ba�ar�l�", $"Foto�raf kaydedildi:\n{fileName}", "Tamam");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private void Notify([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
       
    }
}
