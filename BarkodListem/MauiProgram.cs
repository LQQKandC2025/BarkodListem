using BarkodListem.Data;
using BarkodListem.Pages;
using BarkodListem.Services;
using BarkodListem.ViewModels;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
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
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "barkodlistem.db");
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseBarcodeReader();
            builder.Services.AddSingleton(new DatabaseService(dbPath));
            builder.Services.AddSingleton<BarkodListViewModel>(); // Singleton olarak ekle
            builder.Services.AddTransient<MainPage>(); // MainPage için bağımlılık çözümü
            builder.Services.AddSingleton<WebService>();
            builder.Services.AddSingleton(Plugin.Maui.Audio.AudioManager.Current);
            builder.UseMauiCommunityToolkit();

            // ScannerPage’i de servislere ekleyin:
            builder.Services.AddTransient<IrsaliyeDetayPage>();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}
