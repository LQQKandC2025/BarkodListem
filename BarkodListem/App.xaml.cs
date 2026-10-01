using BarkodListem.Data;
using BarkodListem.Helpers;
using BarkodListem.Services;
using BarkodListem.ViewModels;
using Microsoft.Extensions.DependencyInjection;



namespace BarkodListem
{
    public partial class App : Application
    {
        public static bool IsLoggedIn { get; private set; } = false;
        public static IServiceProvider Services { get; private set; } = null!;
        public static Page? CurrentPage => Current?.Windows.FirstOrDefault()?.Page;
        private static DatabaseService _databaseService = null!;
        private static WebService _webService = null!;

        public App(IServiceProvider services, WebService webService, DatabaseService databaseService)
        {
            InitializeComponent();
            ThemeHelper.ApplyTheme(ThemeHelper.SelectedTheme);
            Services = services;
            _webService=webService;
            _databaseService=databaseService;
            Task.Run(async () => await CopyDbWithDebugInfoAsync());
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new LoginPage());
        }

        public static void LoginSuccessful()
        {
            IsLoggedIn = true;
            SetRootPage(new NavigationPage(new MainPage(
                Services.GetRequiredService<BarkodListViewModel>(), _webService, _databaseService)));
        }

        public static void Logout()
        {
            IsLoggedIn = false;
            SetRootPage(new LoginPage());
        }

        private static void SetRootPage(Page page)
        {
            var window = Current?.Windows.FirstOrDefault();
            if (window != null)
            {
                window.Page = page;
            }
        }
     
        private async Task CopyDbWithDebugInfoAsync()
        {
            try
            {
                var source = Path.Combine(FileSystem.AppDataDirectory, "barkodlistem.db");
                var target = Path.Combine("/sdcard/Download", "barkodlistem_debug.db");

                if (!File.Exists(source))
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                       // await Application.Current.MainPage.DisplayAlert("Dosya Yok", "Kaynak veritabanı dosyası bulunamadı.", "Tamam");
                    });
                    return;
                }

                File.Copy(source, target, true);

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                   // await Application.Current.MainPage.DisplayAlert("Başarılı", "Veritabanı debug konumuna kopyalandı.", "Tamam");
                });
            }
            catch (Exception)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                   // await Application.Current.MainPage.DisplayAlert("HATA", ex.Message, "Tamam");
                });
            }
        }
    }
}
