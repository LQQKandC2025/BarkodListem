using BarkodListem.Pages;
using BarkodListem.Services;
using BarkodListem.ViewModels;
using Plugin.Maui.Audio;
using ZXing.Net.Maui;
using BarkodListem.Services;
namespace BarkodListem.Views
{
    public partial class ScannerPage : ContentPage
    {
        public readonly BarkodListViewModel _viewModel;
        private IAudioManager audioManager;
        private bool isProcessing = false;
        private bool isContinuousMode = false;

        static TaskCompletionSource<string> _tcs;

        public ScannerPage()
            : this(ServiceHelper.GetService<BarkodListViewModel>(),
                   ServiceHelper.GetService<IAudioManager>())
        {
        }
        public ScannerPage(BarkodListViewModel viewModel, IAudioManager audioManager)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.audioManager = audioManager; // Inject AudioManager
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (scanBarcodeReaderView == null)
            {
                DisplayAlert("Hata", "Kamera başlatılamadı. Uygulamanın kamera erişimine izin verdiğinizden emin olun.", "Tamam");
                return;
            }
            scanBarcodeReaderView.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormat.QrCode,
                AutoRotate = true,
                Multiple = false,
                TryHarder = true,
                TryInverted = true
            };
            scanBarcodeReaderView.IsDetecting = true; // 📌 Kamera açıldığında barkodları algılamaya başla
        }
        private void OnToggleContinuousScanClicked(object sender, EventArgs e)
        {
            isContinuousMode = !isContinuousMode; // 📌 Modu değiştir
            toggleContinuousScan.Text = isContinuousMode ? "Sürekli Okuma: Açık" : "Sürekli Okuma: Kapalı";
            toggleContinuousScan.BackgroundColor = isContinuousMode ? Colors.Green : Colors.Gray;
            if (isContinuousMode)
            {
                scanBarcodeReaderView.IsDetecting = true;
            }
        }
        private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            if (isProcessing) return;
            isProcessing = true;
            if (e.Results.Any())
            {
                var barkod = e.Results.FirstOrDefault()?.Value.ToUpper();
                if (!string.IsNullOrEmpty(barkod))
                {
                    try
                    {
                        using (var toneG = new Android.Media.ToneGenerator(Android.Media.Stream.System, 100))
                        {
                            toneG.StartTone(Android.Media.Tone.Dtmf1, 400);
                        }
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine($"Bip sesi çalma hatası: {ex.Message}");
                    }
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        if (barkod.ToUpper().StartsWith("SVK-"))
                        {
                            scanBarcodeReaderView.IsDetecting = false;
                            
                            await Navigation.PushAsync(new SevkiyatDetayPage(barkod));
                        }
                        else
                        {
                            await _viewModel.BarkodEkle(barkod);
                            if (!isContinuousMode)
                            {
                                if (Navigation.NavigationStack.Count > 1)
                                {
                                    await Navigation.PopAsync(); // ❗ sadece normal barkodlar için
                                }
                            }
                            else
                            {
                                await Task.Delay(500);
                                scanBarcodeReaderView.IsDetecting = true;
                            }
                        }
                    });
                }
            }
            isProcessing = false;
        }
        private async Task PlayBeepSound() // Kullanılmıyor. İleride lazım olabilir diye bıraktım.
        {
            try
            {
                var player = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("beep.mp3"));
                player.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ses çalınamadı: " + ex.Message);
            }
        }
        private async void OnCloseClicked(object sender, EventArgs e)
        {
            if (_tcs != null && !_tcs.Task.IsCompleted)
                _tcs.TrySetResult(string.Empty);
        }
        // ScannerPage.xaml.cs içinde
        public static async Task<string> ScanOnceAsync()
        {
            // 1) Yeni TCS
            _tcs = new TaskCompletionSource<string>();

            // 2) ScannerPage örneğini al (DI veya new ScannerPage())
            var scanner = ServiceHelper.GetService<ScannerPage>() ?? new ScannerPage();

            // 3) Modal olarak NavigationPage içinde aç
            var modalNav = new NavigationPage(scanner);
            await Application.Current.MainPage.Navigation.PushModalAsync(modalNav);

            // 4) Sonuç gelene kadar bekle (ya barkod, ya close)
            var result = await _tcs.Task;

            // 5) Modal stack hala doluysa kapat
            if (Application.Current.MainPage.Navigation.ModalStack.Any())
                await Application.Current.MainPage.Navigation.PopModalAsync();

            return result;
        }

    }
}
