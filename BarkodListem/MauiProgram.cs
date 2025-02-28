using Microsoft.Extensions.DependencyInjection;
using BarkodListem.Data;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

using BarkodListem.ViewModels;
using BarkodListem.Services;
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


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
