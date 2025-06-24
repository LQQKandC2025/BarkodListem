using BarkodListem.Data;
using BarkodListem.Helpers;
using BarkodListem.Models;
using BarkodListem.Services;
using BarkodListem.ViewModels;
using CommunityToolkit.Maui.Alerts;
namespace BarkodListem
{
    public partial class App : Application
    {
        public static bool IsLoggedIn { get; private set; } = false;
        public static IServiceProvider? Services { get; private set; }
        private static DatabaseService? _databaseService;
        private static WebService? _webService;
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
        //private async Task CopyDbWithDebugInfoAsync()
        //{
        //    try
        //    {
        //        var source = Path.Combine(FileSystem.AppDataDirectory, "barkodlistem.db");
        //        var target = Path.Combine("/sdcard/Download", "barkodlistem_debug.db");
        //        if (!File.Exists(source))
        //        {
        //            await MainThread.InvokeOnMainThreadAsync(async () =>
        //            {
        //            });
        //            return;
        //        }
        //        File.Copy(source, target, true);
        //        await MainThread.InvokeOnMainThreadAsync(async () =>
        //        {
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        await MainThread.InvokeOnMainThreadAsync(async () =>
        //        {
        //        });
        //    }
        //}

        public static bool IsReleaseMode()
        {
#if RELEASE
            return true;
#else
    return false;
#endif
        }



        public async Task CopyDbWithDebugInfoAsync()
        {
            await Task.Run(() =>
            {
                if (!IsReleaseMode())
                {
                    try
                    {
                        var appDbPath = Path.Combine(FileSystem.AppDataDirectory, "barkodlistem.db");
                        var backupDbPath = Path.Combine("/storage/emulated/0/Download", "barkodlistem_debug.db");
                        var appResimlerPath = Path.Combine(FileSystem.AppDataDirectory, "Resimler");
                        var backupResimlerPath = Path.Combine("/storage/emulated/0/Download", "Resimler");

                        if (File.Exists(appDbPath))
                        {
                            // ✅ Veritabanı varsa --> Yedekleme yap
                            Directory.CreateDirectory("/storage/emulated/0/Download");

                            File.Copy(appDbPath, backupDbPath, true);

                            if (Directory.Exists(appResimlerPath))
                            {
                                if (Directory.Exists(backupResimlerPath))
                                    Directory.Delete(backupResimlerPath, true); // Eski yedek resimleri temizle

                                CopyDirectory(appResimlerPath, backupResimlerPath);
                            }
                        }
                        else
                        {
                            // ✅ Veritabanı yoksa --> Geri yükleme yap
                            if (File.Exists(backupDbPath))
                            {
                                File.Copy(backupDbPath, appDbPath, true);
                            }

                            if (Directory.Exists(backupResimlerPath))
                            {
                                if (Directory.Exists(appResimlerPath))
                                    Directory.Delete(appResimlerPath, true);

                                CopyDirectory(backupResimlerPath, appResimlerPath);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Burada sessiz geçiyoruz çünkü ilk kurulumda klasör olmayabilir
                    }
                }
            });
        }

        public void CopyDirectory(string sourceDir, string targetDir)
        {
            OnTemizlikClicked();
            foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourceDir, targetDir));
            }

            foreach (string filePath in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string destFilePath = filePath.Replace(sourceDir, targetDir);
                File.Copy(filePath, destFilePath, true);
            }
        }
        private async void OnTemizlikClicked()
        {
            var db = await DatabaseService.GetConnectionAsync();
            var kayitliResimler = await db.Table<ResimModel>().ToListAsync();

            var klasorYolu = Path.Combine(FileSystem.AppDataDirectory, "Resimler");
            if (!Directory.Exists(klasorYolu))
                return;

            var tumDosyalar = Directory.GetFiles(klasorYolu, "*.*", SearchOption.AllDirectories);
            int silinenSayisi = 0;

            foreach (var dosya in tumDosyalar)
            {
                var dosyaAdi = Path.GetFileName(dosya);
                if (!kayitliResimler.Any(x => x.DOSYA_ADI == dosyaAdi))
                {
                    try
                    {
                        File.Delete(dosya);
                        silinenSayisi++;
                    }
                    catch
                    {
                        // hata olursa sessiz geç
                    }
                }
            }

            //await DisplayAlert("Temizlik Tamam", $"{silinenSayisi} adet sahipsiz resim silindi.", "Tamam");
        }

    }
}