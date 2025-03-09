using System.Collections.ObjectModel;
using System.Windows.Input;
using BarkodListem.Data;
using BarkodListem.Models;
using BarkodListem.Services;

namespace BarkodListem.ViewModels
{
    public class BarkodListViewModel : BindableObject
    {

        private readonly DatabaseService _databaseService;
        private readonly WebService _webService;

        private string _aktifListeAdi = string.Empty;
        public string girilenListe = string.Empty;
        public string AktifListeAdi
        {
            get => _aktifListeAdi;
            set
            {
                _aktifListeAdi = value;
                OnPropertyChanged(nameof(AktifListeAdi));
            }
        }

        public ObservableCollection<BarkodModel> Barkodlar { get; set; } = new ObservableCollection<BarkodModel>();

        public ICommand BarkodEkleCommand { get; }
        public ICommand BarkodSilCommand { get; }
        public ICommand ListeyiGonderCommand { get; }
        public async Task LoadData()
        {
            if (!string.IsNullOrEmpty(AktifListeAdi))
            {
                var barkodlar = await _databaseService.BarkodlariGetir(AktifListeAdi);
                Barkodlar.Clear();
                foreach (var barkod in barkodlar)
                {
                    Barkodlar.Add(barkod);
                }
            }
        }

        [Obsolete]
        public BarkodListViewModel(DatabaseService databaseService, WebService webService)
        {
            _databaseService = databaseService;
            _webService = webService;
            BarkodEkleCommand = new Command<string>(async (barkod) => await BarkodEkle(barkod));
            BarkodSilCommand = new Command<BarkodModel>(async (barkod) => await BarkodSil(barkod));
            ListeyiGonderCommand = new Command(async () => await ListeyiGonder());




            // 📌 Varsayılan liste adı
            AktifListeAdi = "Geçici Liste";
        }

        public async Task BarkodEkle(string barkod)
        {


            if (!string.IsNullOrEmpty(barkod))
            {
                // Eğer ilk defa ekleniyorsa liste ismi sorulsun
                if (string.IsNullOrEmpty(_aktifListeAdi))
                {

                    if (Application.Current?.Windows.Count > 0)
                    {
                        var page = Application.Current.Windows[0].Page;
                        if (page != null)
                        {
                            string girilenListe = await page.DisplayPromptAsync(
                                "Liste İsmi", "Lütfen liste ismi giriniz:", "Tamam", "İptal", "Liste İsmi");

                            if (string.IsNullOrEmpty(girilenListe))
                                return; // Kullanıcı iptal ederse işlem iptal

                            _aktifListeAdi = girilenListe;
                        }
                    }

                    if (string.IsNullOrEmpty(girilenListe))
                        return; // Kullanıcı iptal ederse işlem iptal
                    _aktifListeAdi = girilenListe;
                }

                var yeniBarkod = new BarkodModel { Barkod = barkod, ListeAdi = _aktifListeAdi };
                await _databaseService.BarkodEkle(yeniBarkod);
                Barkodlar.Insert(0, yeniBarkod);  // Yeni barkod en üste eklensin

                // Liste kaydırma: Yeni eklenen barkod görünür olsun
                await Task.Delay(500);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    int index = Barkodlar.IndexOf(yeniBarkod);
                    if (index != -1)
                    {
                        MainPage.Instance.BarkodListesi.ScrollTo(index, position: ScrollToPosition.Start, animate: true);
                    }
                    else
                    {
                        Console.WriteLine("Hata: Barkod listede yok, kaydırma yapılamadı.");
                    }
                });
            }
        }

        private async Task BarkodSil(BarkodModel barkod)
        {
            if (barkod != null)
            {
                await _databaseService.BarkodSil(barkod);

                var itemToRemove = Barkodlar.FirstOrDefault(b => b.Barkod == barkod.Barkod);
                if (itemToRemove != null)
                {
                    Barkodlar.Remove(itemToRemove);
                }
            }
        }

        [Obsolete]
        private async Task ListeyiGonder()
        {
            if (Barkodlar.Count == 0)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Hata", "Gönderilecek barkod yok!", "Tamam");
                }
                else
                {
                    Console.WriteLine("MainPage şu anda null.");
                }
                return;
            }

            // 📌 Sadece ilk defa liste ismi sorulsun
            if (string.IsNullOrEmpty(_aktifListeAdi))
            {
                if (Application.Current?.MainPage != null)
                {
                    _aktifListeAdi = await Application.Current.MainPage.DisplayPromptAsync(
                    "Liste İsmi", "Lütfen liste ismi giriniz:", "Tamam", "İptal", "Liste İsmi");
                }
                else
                {
                    Console.WriteLine("MainPage şu anda null.");
                }

                if (string.IsNullOrEmpty(_aktifListeAdi)) return; // Kullanıcı iptal ettiyse gönderme
            }

            bool success = await _webService.BarkodListesiGonder(Barkodlar.ToList(), _aktifListeAdi);

            string mesaj = success ? "Liste başarıyla gönderildi!" : "Gönderme başarısız!";
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Bilgi", mesaj, "Tamam");
            }
            else
            {
                Console.WriteLine("MainPage şu anda null.");
            }
        }
        public async Task ClearAllBarkodsAsync()
        {
            bool dbResult = await DatabaseService.DeleteAllBarkodsAsync(); // SQLite'da tüm kayıtları silen metot
            if (dbResult)
            {
                // UI listesini de temizle
                Barkodlar.Clear();
            }
            else
            {
                if (Application.Current?.Windows.Count > 0)
                {
                    var page = Application.Current.Windows[0].Page;
                    if (page != null)
                    {
                        await page.DisplayAlert("Hata", "Kayıtlar silinemedi.", "Tamam"); // MainPage yerine page kullanıldı
                    }
                }
            }
        }

        public async Task SetAktifListe(string yeniListeAdi)
        {
            // Eski değeri saklayın (isteğe bağlı)
            string eskiListeAdi = _aktifListeAdi;

            // Doğrudan _aktifListeAdi'ye atama yerine property setter'ını kullanın:
            AktifListeAdi = yeniListeAdi;  // Bu satır OnPropertyChanged çağırır

            // Veritabanındaki tüm barkodların liste adını güncelleyin
            await _databaseService.UpdateBarkodListeAdi(eskiListeAdi, yeniListeAdi);

            // Yeni listeye ait barkodları veritabanından çekip UI'yi güncelleyin:
            var yeniBarkodlar = await _databaseService.BarkodlariGetir(AktifListeAdi);
            Barkodlar.Clear();
            foreach (var barkod in yeniBarkodlar)
            {
                Barkodlar.Add(barkod);
            }
        }

    }
}
