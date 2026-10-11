using Android.App;
using Android.Content.PM;
using Android.OS;
namespace BarkodListem
{
    [Activity(Label = "Mobil Servis",
              Theme = "@style/Maui.SplashTheme",
              MainLauncher = true,
              LaunchMode = LaunchMode.SingleTop,
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#071C2E"));
            Window.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#F3F6F9"));
        }
    }
}
