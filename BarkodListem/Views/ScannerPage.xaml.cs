using BarkodListem.Pages;
using BarkodListem.ViewModels;
using Plugin.Maui.Audio;
using ZXing.Net.Maui;

namespace BarkodListem.Views
{
    public partial class ScannerPage : ContentPage
    {
        public readonly BarkodListViewModel _viewModel;
        private IAudioManager audioManager;
        private bool isProcessing = false;
        private bool isContinuousMode = false;

        // 📌 **Doğru constructor:**
        public ScannerPage(BarkodListViewModel viewModel, IAudioManager audioManager)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.audioManager = audioManager; // Inject AudioManager
                                              // scanBarcodeReaderView.BarcodesDetected += OnBarcodesDetected;
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
                Formats = BarcodeFormats.All, // Sadece QR kodları oku
                AutoRotate = true,
                Multiple = false,
                TryHarder = true,
                TryInverted = true
            };

            scanBarcodeReaderView.IsDetecting = true; // 📌 Kamera açıldığında barkodları algılamaya başla
        }

        // 📌 Barkod Algılandığında Çalışacak Metod
        private void OnToggleContinuousScanClicked(object sender, EventArgs e)
        {
            isContinuousMode = !isContinuousMode; // 📌 Modu değiştir

            // 📌 Butonun yazısını güncelle
            toggleContinuousScan.Text = isContinuousMode ? "Sürekli Okuma: Açık" : "Sürekli Okuma: Kapalı";
            toggleContinuousScan.BackgroundColor = isContinuousMode ? Colors.Green : Colors.Gray;

            // 📌 Eğer sürekli okuma açıksa, kamerayı algılamaya devam et
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
                var barkod = e.Results.FirstOrDefault()?.Value;
                if (!string.IsNullOrEmpty(barkod))
                {
                    try
                    {
                        using (var toneG = new Android.Media.ToneGenerator(Android.Media.Stream.System, 100))
                        {
                            toneG.StartTone(Android.Media.Tone.Dtmf1, 200);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Bip sesi çalma hatası: {ex.Message}");
                    }

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        if (barkod.StartsWith("SVK-"))
                        {
                            // 📌 Listeye ekleme yerine sevkiyat formuna yönlendir
                            await Navigation.PushAsync(new SevkiyatDetayPage(barkod));

                            // Eğer sürekli okuma modu kapalıysa sayfayı kapatalım
                            if (!isContinuousMode && Navigation.NavigationStack.Count > 1)
                            {
                                await Navigation.PopAsync(); // ScannerPage kapanır
                            }
                        }
                        else
                        {
                            // Barkod listeye eklenecek
                            await _viewModel.BarkodEkle(barkod);

                            if (!isContinuousMode)
                            {
                                if (Navigation.NavigationStack.Count > 1)
                                {
                                    await Navigation.PopAsync();
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




        // 📢 Play Beep Sound
        private async Task PlayBeepSound()
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

        // 📌 Kapat Butonu İçin Event Handler
        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(); // Sayfayı kapat
        }
    }
}
