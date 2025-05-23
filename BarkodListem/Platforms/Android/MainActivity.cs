﻿using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Graphics;

namespace BarkodListem
{
    [Activity(Label = "BarkodListem",
              Theme = "@style/Maui.SplashTheme",
              MainLauncher = true,
              LaunchMode = LaunchMode.SingleTop,
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // 🔹 Üst bar rengini ayarla (yarı şeffaf açık mavi)
            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#BB2196F3")); // Değiştirilebilir
        }
    }
}
