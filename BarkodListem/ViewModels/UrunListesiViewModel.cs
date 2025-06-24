using BarkodListem.Data;
using BarkodListem.Models;
using BarkodListem.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
namespace BarkodListem.ViewModels
{
    public class UrunListesiViewModel : INotifyPropertyChanged
    {
        public Boolean SORUNVAR { get; set; } = false;
        public ObservableCollection<UrunModel> Urunler { get; set; } = new();
        public ICommand FotografCekCommand { get; set; }
        public ICommand FotografSSHCommand { get; set; }
        public ICommand UrunLongPressCommand { get; set; } // 👈 Yeni ekledik
        private readonly WebService _webService;
        private string _sevkiyatNo;
        public string dbPath = Path.Combine(FileSystem.AppDataDirectory, "barkodlistem.db");
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand GridTappedCommand { get; set; }
        public UrunListesiViewModel()
        {
            var databaseService = new DatabaseService(dbPath);
            _webService = new WebService(databaseService);
            FotografCekCommand = new Command<UrunModel>(async (urun) => await FotografCekAsync(urun, "TF"));
            FotografSSHCommand = new Command<UrunModel>(async (urun) => await FotografCekAsync(urun, "SSH"));
            UrunLongPressCommand = new Command<UrunModel>(async (urun) => await OnUrunLongPressAsync(urun));
        }
        public async Task YukleAsync(string sevkiyatNo)
        {
            _sevkiyatNo = sevkiyatNo;
            var dt = await _webService.SevkiyatUrunListesi(sevkiyatNo);
            Urunler.Clear();
            int siraNo = 1;
            var db = await DatabaseService.GetConnectionAsync();
            foreach (System.Data.DataRow row in dt.Rows)
            {
                var stokId = int.Parse(row["STOK_ID"].ToString());
                var sevkFisId = int.Parse(row["SEVK_FIS_ID"].ToString());
                var sshDetay = await db.Table<SSHDetayModel>()
                                       .FirstOrDefaultAsync(x => x.SEVKIYAT_NO == sevkiyatNo &&
                                                                 x.STOK_ID == stokId &&
                                                                 x.SEVK_FIS_ID == sevkFisId);
                bool SORUNVAR = sshDetay != null;
                Urunler.Add(new UrunModel
                {
                    STOK_ADI = row["STOK_ADI"].ToString(),
                    STOK_SPEC = row["STOK_SPEC"].ToString(),
                    MIKTAR = Convert.ToDecimal(row["MIKTAR"]),
                    SEVK_ARAC_ID = row["SEVK_ARAC_ID"].ToString(),
                    SIP_STR_ID = row["SIP_STR_ID"].ToString(),
                    SEVK_FIS_ID = row["SEVK_FIS_ID"].ToString(),
                    STOK_ID = row["STOK_ID"].ToString(),
                    SUBE_KODU = row["SUBE_KODU"].ToString(),
                    CARI_ID = row["CARI_ID"].ToString(),
                    SEVKIYAT_NO = sevkiyatNo,
                    Sirano = siraNo,
                    SORUNVAR = SORUNVAR
                });
                siraNo++;
            }
            Notify(nameof(Urunler));
        }
        public async Task FotografCekAsync(UrunModel urun, string tip)
        {
            try
            {
                var result = await MediaPicker.CapturePhotoAsync();
                if (result == null) return;
                var klasorYolu = Path.Combine(FileSystem.AppDataDirectory, "Resimler", _sevkiyatNo);
                Directory.CreateDirectory(klasorYolu);
                int sira = Directory.GetFiles(klasorYolu, $"TF_{urun.SEVK_FIS_ID}_{urun.STOK_ID}_{urun.SIP_STR_ID}_*.jpg").Length + 1;
                var fileName = $"TF_{urun.SEVK_FIS_ID}_{urun.STOK_ID}_{urun.SIP_STR_ID}_{sira}.jpg";
                var fullPath = Path.Combine(klasorYolu, fileName);
                using var stream = await result.OpenReadAsync();
                using var fileStream = File.OpenWrite(fullPath);
                await stream.CopyToAsync(fileStream);
                var db = await DatabaseService.GetConnectionAsync();
                var model = new ResimModel
                {
                    SEVKIYAT_NO = urun.SEVKIYAT_NO,
                    STOK_ID= int.Parse(urun.STOK_ID),
                    SIPSTR_ID= int.Parse(urun.SIP_STR_ID),
                    DOSYA_ADI = fileName,
                    YOL="",
                    TARIH = DateTime.Now,
                    TIPI = "TESLİMAT"
                };
                await db.InsertAsync(model);
                await Application.Current.MainPage.DisplayAlert("Resim", $"Resim kaydedildi:\n{fileName}", "Tamam");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("HATA", ex.Message, "Tamam");
            }
        }
        private async Task OnUrunLongPressAsync(UrunModel urun)
        {
            if (urun == null) return;
            if (!urun.SORUNVAR)
                return;
            bool cevap = await Application.Current.MainPage.DisplayAlert(
                "SSH Kayıtlarını Sil",
                $"Seçilen ürün ({urun.STOK_ADI}) için SSH kayıtlarını silmek istiyor musunuz?",
                "Evet", "Hayır");
            if (!cevap)
                return; // Hayır'a basarsa çık.
            var db = await DatabaseService.GetConnectionAsync();
            // 1. SSH_DETAY kaydını sil
            int stokId = int.Parse(urun.STOK_ID);
            int sevkFisId = int.Parse(urun.SEVK_FIS_ID);

            // Sonra Where içinde kullan
            var silinecekKayitlar = await db.Table<SSHDetayModel>()
                .Where(x => x.SEVKIYAT_NO == urun.SEVKIYAT_NO
                         && x.STOK_ID == stokId
                         && x.SEVK_FIS_ID == sevkFisId)
                .ToListAsync();
            foreach (var kayit in silinecekKayitlar)
            {
                await db.DeleteAsync(kayit);
            }
            // 2. SSH tipi resimleri sil
            var klasorYolu = Path.Combine(FileSystem.AppDataDirectory, "Resimler", urun.SEVKIYAT_NO);
            if (Directory.Exists(klasorYolu))
            {
                var sshResimler = Directory.GetFiles(klasorYolu);
                foreach (var dosya in sshResimler)
                {
                    var dosyaAdi = Path.GetFileName(dosya);
                    if (dosyaAdi.StartsWith($"SSH_{urun.SEVK_FIS_ID}_{urun.STOK_ID}_{urun.SIP_STR_ID}_"))
                    {
                        try
                        {
                            File.Delete(dosya);

                            // 🔥 RESIMLER tablosundan da sil
                            try { await db.ExecuteAsync("DELETE FROM RESIMLER WHERE DOSYA_ADI = ?", dosyaAdi); } catch { /* hata olursa boşver */ }
                        }
                        catch (Exception ex)
                        {
                            await Application.Current.MainPage.DisplayAlert("HATA", $"Resim silinirken hata oluştu: {ex.Message}", "Tamam");
                        }
                    }
                }
            }


            urun.SORUNVAR = false;


            Notify(nameof(Urunler));

            if (Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault() is Pages.UrunListesiPage urunListesiPage
    && urunListesiPage.BindingContext is ViewModels.UrunListesiViewModel viewModel)
            {
                viewModel.Notify(nameof(viewModel.Urunler));
                urunListesiPage.RefreshCollectionView(); // 🔥 Buraya RefreshCollectionView çağıracağız
            }
            await Application.Current.MainPage.DisplayAlert("Başarılı", "SSH kayıtları ve ilgili resimler silindi.", "Tamam");

            // İstersen Urunler listesini yenileyebilirsin burada.


        }
        public void Notify([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string GetSevkiyatNo()
        {
            return _sevkiyatNo;
        }
        public void RefreshUrun(UrunModel urun)
        {
            if (Urunler.Contains(urun))
            {
                var index = Urunler.IndexOf(urun);
                Urunler.RemoveAt(index);
                Urunler.Insert(index, urun);
            }
        }

    }
}
