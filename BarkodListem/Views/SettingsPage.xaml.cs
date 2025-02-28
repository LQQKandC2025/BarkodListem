
using BarkodListem.Helpers;

using BarkodListem.Models;
using BarkodListem.Data;

namespace BarkodListem.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        public SettingsPage(DatabaseService databaseService)
        {
            InitializeComponent();

            // XAML'deki Entry nesneleri burada kullanılabilir olmalı
            entryUrl.Text = SettingsHelper.WebServiceUrl;
            entryPort.Text = SettingsHelper.WebServicePort;
            entryUser.Text = SettingsHelper.UserName;
            entryPassword.Text = SettingsHelper.Password;

            // Tema seçimini yükle
            themePicker.SelectedIndex = (ThemeHelper.SelectedTheme == AppTheme.Dark) ? 1 : 0;
            _databaseService=databaseService;
        }

        private async void Kaydet_Clicked(object sender, EventArgs e)
        {
            var ayarlar = new AyarlarModel
            {
                WebServisURL = entryUrl.Text,
                Port = int.Parse(entryPort.Text),
                KullaniciAdi = entryUser.Text,
                Sifre = entryPassword.Text
            };

            await _databaseService.AyarKaydet(ayarlar);  // 📌 Yeni metod eklendi, SQLite'a kaydedecek
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

