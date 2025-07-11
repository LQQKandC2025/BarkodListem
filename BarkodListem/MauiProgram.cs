using BarkodListem.Data;
using BarkodListem.Pages;
using BarkodListem.Services;
using BarkodListem.ViewModels;
using BarkodListem.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace BarkodListem
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
#if ANDROID
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
                handler.PlatformView.Background = null;
            });
#endif
            // Veritabanı yolu
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "barkodlistem.db");

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()      // sadece bir kez
                .UseBarcodeReader()             // ZXing.Net.Maui için
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Servis kayıtları
            builder.Services.AddSingleton(new DatabaseService(dbPath));
            builder.Services.AddSingleton<WebService>();
            builder.Services.AddSingleton<BarkodListViewModel>();   // tekil VM
            builder.Services.AddTransient<MainPage>();              // giriş sayfası
            builder.Services.AddTransient<ScannerPage>();           // barkod okuma sayfası
            builder.Services.AddTransient<IrsaliyeDetayPage>();     // detaya geçiş sayfası

            // Ses oynatma yöneticisi
            builder.Services.AddSingleton<IAudioManager, AudioManager>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}
