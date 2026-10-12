using BarkodListem.Pages;
using BarkodListem.Services;
using BarkodListem.ViewModels;
using Plugin.Maui.Audio;
using ZXing.Net.Maui;
namespace BarkodListem.Views
{
    public enum ScannerMode
    {
        Sayim,
        SevkiyatTekOkuma,
        IrsaliyeTekOkuma
    }

    public partial class ScannerPage : ContentPage
    {
        public readonly BarkodListViewModel _viewModel;
        private IAudioManager audioManager;
        private bool isProcessing = false;
        private bool isContinuousMode = false;
        private readonly ScannerMode _scannerMode;

        private TaskCompletionSource<string>? _scanOnceCompletionSource;

        public ScannerPage()
            : this(ServiceHelper.GetService<BarkodListViewModel>(),
                   ServiceHelper.GetService<IAudioManager>())
        {
        }
        public ScannerPage(BarkodListViewModel viewModel, IAudioManager audioManager)
            : this(viewModel, audioManager, ScannerMode.Sayim)
        {
        }
        public ScannerPage(BarkodListViewModel viewModel, IAudioManager audioManager, ScannerMode scannerMode)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.audioManager = audioManager; // Inject AudioManager
            _scannerMode = scannerMode;

            bool sayimModu = scannerMode == ScannerMode.Sayim;
            toggleContinuousScan.IsVisible = sayimModu;
            scannerTitle.Text = scannerMode switch
            {
                ScannerMode.Sayim => "BARKOD TARAYICI",
                ScannerMode.SevkiyatTekOkuma => "SEVKİYAT TARAYICI",
                _ => "PAKET TARAYICI"
            };
            scannerSubtitle.Text = scannerMode switch
            {
                ScannerMode.Sayim => "Kodu çerçevenin içine hizalayın",
                ScannerMode.SevkiyatTekOkuma => "Tek bir SVK sevkiyat barkodu okutun",
                _ => "Tek bir paket barkodu okutun"
            };
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
                Formats = BarcodeFormats.OneDimensional | BarcodeFormat.QrCode,
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
            try
            {
                if (!e.Results.Any())
                    return;

                var barkod = e.Results.FirstOrDefault()?.Value?.Trim().ToUpperInvariant();
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
                        if (_scannerMode == ScannerMode.IrsaliyeTekOkuma)
                        {
                            scanBarcodeReaderView.IsDetecting = false;
                            _scanOnceCompletionSource?.TrySetResult(barkod);
                            return;
                        }

                        if (_scannerMode == ScannerMode.SevkiyatTekOkuma)
                        {
                            scanBarcodeReaderView.IsDetecting = false;
                            if (!barkod.StartsWith("SVK-", StringComparison.OrdinalIgnoreCase))
                            {
                                await DisplayAlert(
                                    "Geçersiz Barkod",
                                    "Ana menüde yalnızca SVK ile başlayan sevkiyat barkodları okutulabilir.",
                                    "Tamam");
                                await Navigation.PopAsync();
                                return;
                            }

                            await Navigation.PushAsync(new SevkiyatDetayPage(barkod));
                            Navigation.RemovePage(this);
                            return;
                        }

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
            finally
            {
                isProcessing = false;
            }
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
            if (_scanOnceCompletionSource != null && !_scanOnceCompletionSource.Task.IsCompleted)
            {
                scanBarcodeReaderView.IsDetecting = false;
                _scanOnceCompletionSource.TrySetResult(string.Empty);
                return;
            }

            if (Navigation.NavigationStack.Count > 1)
                await Navigation.PopAsync();
        }
        // ScannerPage.xaml.cs içinde
        public static async Task<string> ScanOnceAsync()
        {
            var scanner = new ScannerPage(
                ServiceHelper.GetService<BarkodListViewModel>(),
                ServiceHelper.GetService<IAudioManager>(),
                ScannerMode.IrsaliyeTekOkuma)
            {
                _scanOnceCompletionSource = new TaskCompletionSource<string>(
                    TaskCreationOptions.RunContinuationsAsynchronously)
            };

            var modalNav = new NavigationPage(scanner);
            await Application.Current.MainPage.Navigation.PushModalAsync(modalNav);

            var result = await scanner._scanOnceCompletionSource.Task;

            if (Application.Current.MainPage.Navigation.ModalStack.Any())
                await Application.Current.MainPage.Navigation.PopModalAsync();

            return result;
        }

    }
}
