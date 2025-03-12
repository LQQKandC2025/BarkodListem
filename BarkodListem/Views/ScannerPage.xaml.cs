using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using BarkodListem.ViewModels;
using BarkodListem;
using Microsoft.Maui.Controls;
using System.Linq;

namespace BarkodListem.Views
{
    public partial class ScannerPage : ContentPage
    {
        public readonly BarkodListViewModel _viewModel;

        // 📌 **Doğru constructor:**
        public ScannerPage(BarkodListViewModel _viewModel)
        {
            InitializeComponent();
           

            // Kamera tarama başlatılsın
            scanBarcodeReaderView.BarcodesDetected += OnBarcodesDetected;
     
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
        private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            if (e.Results.Any())
            {
                var barkod = e.Results.FirstOrDefault()?.Value;

                if (!string.IsNullOrEmpty(barkod))
                {
                    _viewModel.BarkodEkleCommand.Execute(barkod);

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await DisplayAlert("Barkod Okundu", barkod, "Tamam");
                        await Navigation.PopAsync(); // Sayfayı kapat
                    });
                }
            }
        }

        // 📌 Kapat Butonu İçin Event Handler
        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(); // Sayfayı kapat
        }
    }
}
