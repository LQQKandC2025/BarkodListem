﻿using ZXing.Net.Maui;
using BarkodListem.ViewModels;
using BarkodListem.Models;
using BarkodListem.Views;
using BarkodListem.Data;
using BarkodListem.Services;
using Plugin.Maui.Audio;



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
            //#if ANDROID // 📌 Yalnızca Android için çalıştır
            //            if (isScanning)
            //                return;

            //            isScanning = true;

            //            try
            //            {
            //                var scanPage = new ZXing.Net.Maui.Controls.CameraBarcodeReaderView();

            //                if (DeviceInfo.Version.Major >= 33) // 📌 Android 33 ve üstü için BarcodeReaderOptions kullan
            //                {
            //                    scanPage.Options = new ZXing.Net.Maui.BarcodeReaderOptions
            //                    {
            //                        Formats = BarcodeFormats.All,
            //                        AutoRotate = true,  // Kameranın yönünü otomatik algıla
            //                        Multiple = false,   // Aynı anda birden fazla kod okuma kapalı
            //                        TryHarder = true,  // Düşük kontrastlı kodları algılamak için ekstra çaba göster
            //                        TryInverted = true, // Ters renkli (negatif) QR kodları da oku       
            //                    };
            //                }

            //                HashSet<string> tarananBarkodlar = new HashSet<string>();

            //                scanPage.BarcodesDetected += (s, e) =>
            //                {
            //                    Device.BeginInvokeOnMainThread(() =>
            //                    {
            //                        if (e.Results.Any())
            //                        {
            //                            var result = e.Results.FirstOrDefault()?.Value;

            //                            if (!string.IsNullOrEmpty(result) && !tarananBarkodlar.Contains(result))
            //                            {
            //                                tarananBarkodlar.Add(result);
            //                                _viewModel.BarkodEkleCommand.Execute(result);

            //                                // Barkod okunduğunda bip sesi çal
            //                                try
            //                                {
            //                                    using (var toneG = new Android.Media.ToneGenerator(Android.Media.Stream.System, 100))
            //                                    {
            //                                        toneG.StartTone(Android.Media.Tone.Dtmf1, 200); // 200 ms süreli bip
            //                                    }
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    Console.WriteLine($"Bip sesi çalma hatası: {ex.Message}");
            //                                }

            //                                Navigation.PopAsync();
            //                            }
            //                        }
            //                    });
            //                };

            //                await Navigation.PushAsync(new ContentPage { Content = scanPage });
            //            }
            //            catch (Exception ex)
            //            {
            //                await DisplayAlert("Hata", $"Barkod okuma hatası: {ex.Message}", "Tamam");
            //            }
            //            finally
            //            {
            //                isScanning = false;
            //            }
            //#else
            //    await DisplayAlert("Hata", "Barkod tarayıcı yalnızca Android'de destekleniyor.", "Tamam");
            //#endif
        }



        private void BarkodEkle_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(barkodEntry.Text))
            {


                _viewModel.BarkodEkleCommand.Execute(barkodEntry.Text);
                barkodEntry.Text = string.Empty; // Giriş kutusunu temizle
                barkodEntry.Focus(); // 📌 İmleci tekrar giriş kutusuna getir

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
                mevcutListeAdi == "Geçici Liste" ? "" : mevcutListeAdi // Geçici Liste ise boş bırak
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

        private void LogoutButton_Clicked(object sender, EventArgs e)
        {
            App.Logout(); // Kullanıcıyı çıkış yaptır
        }

    }
}