using BarkodListem.Data;
using BarkodListem.Helpers;
using BarkodListem.Services;
using BarkodListem.ViewModels;



namespace BarkodListem
{
    public partial class App : Application
    {
        public static bool IsLoggedIn { get; private set; } = false;
        public static IServiceProvider Services { get; private set; }
        private static DatabaseService _databaseService;
        private static WebService _webService;

        [System.Obsolete]
        public App(IServiceProvider services, WebService webService, DatabaseService databaseService)
        {
            InitializeComponent();
            ThemeHelper.ApplyTheme(ThemeHelper.SelectedTheme);
            Services = services;
            _webService=webService;
            _databaseService=databaseService;
            MainPage = new LoginPage();
            Task.Run(async () => await CopyDbWithDebugInfoAsync());
            //MainPage = new NavigationPage(new MainPage(Services.GetService<BarkodListViewModel>(),_webService,_databaseService)); // ✅ NavigationPage kullan
        }
        [System.Obsolete]
        public static void LoginSuccessful()
        {
            IsLoggedIn = true;

            Application.Current.MainPage = new NavigationPage(new MainPage(Services.GetService<BarkodListViewModel>(), _webService, _databaseService));
        }
        [System.Obsolete]
        public static void Logout()
        {
            IsLoggedIn = false;
            Application.Current.MainPage = new LoginPage();
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
                        await Application.Current.MainPage.DisplayAlert("Dosya Yok", "Kaynak veritabanı dosyası bulunamadı.", "Tamam");
                    });
                    return;
                }

                File.Copy(source, target, true);

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Başarılı", "Veritabanı debug konumuna kopyalandı.", "Tamam");
                });
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("HATA", ex.Message, "Tamam");
                });
            }
        }

        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new AppShell());
        //}
    }
}