using BarkodListem.Data;
using BarkodListem.Models;
using BarkodListem.Services;
using BarkodListem.ViewModels;
using BarkodListem.Views;
using Plugin.Maui.Audio;
using BarkodListem.Pages;



namespace BarkodListem
{
    public partial class MainPage : ContentPage
    {
        public static MainPage Instance { get; private set; }
        public readonly BarkodListViewModel _viewModel;
        private readonly WebService _webService;
        private readonly DatabaseService _databaseService;
        public MainPage(BarkodListViewModel viewModel, WebService webService, DatabaseService databaseService)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
            _webService=webService;
            Instance = this;
            _databaseService=databaseService;

        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadData();
        }


        public CollectionView BarkodListesi => barkodListesi;
        // 📌 QR Kodu Okuma İşlemi
        private bool isScanning = false;

        [Obsolete]
        private async void QRKodTara_Clicked(object sender, EventArgs e)
        {

            var audioManager = ServiceHelper.GetService<IAudioManager>(); // 📌 Get AudioManager
            await Navigation.PushAsync(new ScannerPage(_viewModel, audioManager)); // Pass it to ScannerPage

        }



        private void BarkodEkle_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(barkodEntry.Text))
            {
                barkodEntry.Text= barkodEntry.Text.Trim().ToUpper(); 

                if (barkodEntry.Text.StartsWith("SVK-"))
                {
                    // Sevkiyat formuna yönlendir
                    Navigation.PushAsync(new SevkiyatDetayPage(barkodEntry.Text));
                    barkodEntry.Text = string.Empty; // Giriş kutusunu temizle
                    barkodEntry.Focus(); // 📌 İmleci tekrar giriş kutusuna getir
                } else
                {
                    _viewModel.BarkodEkleCommand.Execute(barkodEntry.Text);
                    barkodEntry.Text = string.Empty; // Giriş kutusunu temizle
                    barkodEntry.Focus(); // 📌 İmleci tekrar giriş kutusuna getir
                }
            }
        }
        private async void YeniListeOlustur_Clicked(object sender, EventArgs e)
        {
            string yeniListeAdi = await DisplayPromptAsync("Yeni Liste", "Yeni liste ismi giriniz:", "Tamam", "İptal");

            if (!string.IsNullOrWhiteSpace(yeniListeAdi))
            {
                await _viewModel.YeniListeOlustur(yeniListeAdi);
                await DisplayAlert("Başarılı", $"Yeni liste oluşturuldu: {yeniListeAdi}", "Tamam");
            }
        }
        private void ResimGonder_Clicked(object sender, EventArgs e)
        {

        }
        // 📌 Barkod Seçip Silme İşlemi
        private async void BarkodSilCommand(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is BarkodModel selectedBarkod)
            {
                bool confirm = await DisplayAlert("Silme Onayı", "Seçili barkodu silmek istiyor musunuz?", "Evet", "Hayır");
                if (confirm)
                {
                    _viewModel.BarkodSilCommand.Execute(selectedBarkod);
                }
            }
        }

        // 📌 Listeyi Web Servise Gönderme İşlemi
        //private async void ListeyiGonder_Clicked(object sender, EventArgs e)
        //{
        //    bool confirm = await DisplayAlert("Gönderim Onayı", "Listeyi web servise göndermek istiyor musunuz?", "Evet", "Hayır");
        //    if (confirm)
        //    {
        //        await Task.Run(() => _viewModel.ListeyiGonderCommand.Execute(null));
        //    }
        //}

        private async void ListeyiGonder_Clicked(object sender, EventArgs e)
        {
            // 📌 Mevcut liste adını al
            string mevcutListeAdi = _viewModel.AktifListeAdi;

            // 📌 Eğer liste "Geçici Liste" ise, yeni bir isim iste
            string listeAdi = await DisplayPromptAsync("Liste İsmi",
                "Barkodları göndermek için bir liste ismi giriniz:",
                "Tamam", "İptal",
                mevcutListeAdi == "Geçici Liste" ? _viewModel.AktifListeAdi : mevcutListeAdi // Geçici Liste ise boş bırak
            );

            if (string.IsNullOrEmpty(listeAdi))
            {
                listeAdi = _viewModel.AktifListeAdi;
            }

            bool confirm = await DisplayAlert("Gönderim Onayı", $"{listeAdi} listesini web servise göndermek istiyor musunuz?", "Evet", "Hayır");
            if (confirm)
            {
                bool success = await _webService.BarkodListesiGonder(_viewModel.Barkodlar.ToList(), listeAdi);
                string mesaj = success ? "Liste başarıyla gönderildi!" : "Gönderme başarısız!";
                await DisplayAlert("Bilgi", mesaj, "Tamam");
            }

        }
        private async void Ayarlar_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage(_databaseService));
        }

        private async void ClearListButton_Clicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Listeyi Temizle", "Tüm kayıtları silmek istediğinizden emin misiniz?", "Evet", "Hayır");
            if (confirm)
            {
                // ViewModel üzerinden hem veritabanını temizle hem de in-memory listeden kayıtları sil
                await _viewModel.ClearAllBarkodsAsync();
                
            }
        }
        private void BarkodEntry_Completed(object sender, EventArgs e)
        {
            // BARKOD KAYDET butonunun Clicked ile aynı işlemi
            BarkodEkle_Clicked(sender, e);
            barkodEntry.Focus(); // 📌 İmleci tekrar giriş kutusuna getir
        }
        private async void Listeler_Clicked(object sender, EventArgs e)
        {
            // DI üzerinden DatabaseService örneğini al (using Microsoft.Extensions.DependencyInjection)
            //var dbService = App.Services.GetService<DatabaseService>();
            //await Navigation.PushAsync(new ListelerPage(dbService));
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "barkodlistem.db");
            var dbService = new DatabaseService(dbPath);
            await Navigation.PushAsync(new ListelerPage(dbService, _viewModel, _webService));

        }
        private async void ListeyiDegistir_Clicked(object sender, EventArgs e)
        {
            string yeniListeAdi = await DisplayPromptAsync(
                "Yeni Liste", "Yeni liste ismi giriniz:", "Tamam", "İptal", "Liste İsmi");

            if (!string.IsNullOrEmpty(yeniListeAdi))
            {
                await _viewModel.SetAktifListe(yeniListeAdi);
            }
        }
        private async void ListeAdiniDegistir_Tapped(object sender, EventArgs e)
        {
            string yeniListeAdi = await DisplayPromptAsync("Yeni Liste", "Yeni liste ismi giriniz:", "Tamam", "İptal", "Liste İsmi");
            if (!string.IsNullOrEmpty(yeniListeAdi))
            {
                await _viewModel.SetAktifListe(yeniListeAdi);
            }
            else
            {
                await _viewModel.SetAktifListe(_viewModel.AktifListeAdi); ;
            }
        }
        [Obsolete]
        private void LogoutButton_Clicked(object sender, EventArgs e)
        {
            App.Logout(); // Kullanıcıyı çıkış yaptır
        }

    }
}