using BarkodListem.Data;
using BarkodListem.Models;
using BarkodListem.Pages;
using BarkodListem.Services;
using BarkodListem.ViewModels;
using BarkodListem.Views;
using Plugin.Maui.Audio;
namespace BarkodListem
{
    public partial class MainPage : ContentPage
    {
        public static MainPage Instance { get; private set; }
        public readonly BarkodListViewModel _viewModel;
        private readonly WebService _webService;
        private readonly DatabaseService _databaseService;
        private CancellationTokenSource? _serviceStatusCancellation;
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
            ServisDurumKontrolunuBaslat();
        }
        protected override void OnDisappearing()
        {
            _serviceStatusCancellation?.Cancel();
            _serviceStatusCancellation?.Dispose();
            _serviceStatusCancellation = null;
            base.OnDisappearing();
        }
        private void ServisDurumKontrolunuBaslat()
        {
            _serviceStatusCancellation?.Cancel();
            _serviceStatusCancellation?.Dispose();
            _serviceStatusCancellation = new CancellationTokenSource();
            _ = ServisDurumunuPeriyodikKontrolEtAsync(_serviceStatusCancellation.Token);
        }
        private async Task ServisDurumunuPeriyodikKontrolEtAsync(CancellationToken cancellationToken)
        {
            await ServisDurumunuGuncelleAsync(cancellationToken);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
            try
            {
                while (await timer.WaitForNextTickAsync(cancellationToken))
                    await ServisDurumunuGuncelleAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Sayfadan ayrılınca periyodik kontrol normal olarak sonlanır.
            }
        }
        private async Task ServisDurumunuGuncelleAsync(CancellationToken cancellationToken)
        {
            bool bagli;
            try
            {
                bagli = await _webService.ServisBaglantisiniKontrolEtAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
                return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (bagli)
                {
                    serviceStatusLabel.Text = "Servise bağlı";
                    serviceStatusLabel.TextColor = Color.FromArgb("#176B56");
                    serviceStatusDetailLabel.Text = $"Son kontrol: {DateTime.Now:HH:mm:ss}";
                    serviceStatusIcon.TextColor = Color.FromArgb("#20A47A");
                    serviceStatusIconBorder.BackgroundColor = Color.FromArgb("#E4F6EF");
                }
                else
                {
                    serviceStatusLabel.Text = "Servise bağlantı yok";
                    serviceStatusLabel.TextColor = Color.FromArgb("#B33A46");
                    serviceStatusDetailLabel.Text = $"Son deneme: {DateTime.Now:HH:mm:ss} · Ayarları kontrol edin";
                    serviceStatusIcon.TextColor = Color.FromArgb("#D94A55");
                    serviceStatusIconBorder.BackgroundColor = Color.FromArgb("#FBEAEC");
                }
            });
        }
        private async void Sayim_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SayimPage(_viewModel, _webService, _databaseService));
        }
        //private bool isScanning = false;
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
                    Navigation.PushAsync(new SevkiyatDetayPage(barkodEntry.Text));
                    barkodEntry.Text = string.Empty; // Giriş kutusunu temizle
                    barkodEntry.Focus(); // 📌 İmleci tekrar giriş kutusuna getir
                }
                else
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
        private async void ListeyiGonder_Clicked(object sender, EventArgs e)
        {
            string mevcutListeAdi = _viewModel.AktifListeAdi;
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
                await _viewModel.ClearAllBarkodsAsync();
            }
        }
        private void BarkodEntry_Completed(object sender, EventArgs e)
        {
            BarkodEkle_Clicked(sender, e);
            barkodEntry.Focus(); // 📌 İmleci tekrar giriş kutusuna getir
        }
        private async void Listeler_Clicked(object sender, EventArgs e)
        {
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
        private async void Irsaliye_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new IrsaliyeListPage());
        }
    }
}
