using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarkodListem.Views;
using BarkodListem.Helpers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;


namespace BarkodListem.Views
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();

            // XAML'deki Entry nesneleri burada kullanılabilir olmalı
            entryUrl.Text = SettingsHelper.WebServiceUrl;
            entryPort.Text = SettingsHelper.WebServicePort;
            entryUser.Text = SettingsHelper.UserName;
            entryPassword.Text = SettingsHelper.Password;

            // Tema seçimini yükle
            themePicker.SelectedIndex = (ThemeHelper.SelectedTheme == AppTheme.Dark) ? 1 : 0;
        }

        private async void Kaydet_Clicked(object sender, EventArgs e)
        {
            SettingsHelper.WebServiceUrl = entryUrl.Text;
            SettingsHelper.WebServicePort = entryPort.Text;
            SettingsHelper.UserName = entryUser.Text;
            SettingsHelper.Password = entryPassword.Text;

            await DisplayAlert("Başarılı", "Ayarlar kaydedildi!", "Tamam");
        }

        private void ThemePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (themePicker.SelectedIndex == 0)
                ThemeHelper.SelectedTheme = AppTheme.Light;
            else
                ThemeHelper.SelectedTheme = AppTheme.Dark;
        }
    }
}

