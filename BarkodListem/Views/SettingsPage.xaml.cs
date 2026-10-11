using BarkodListem.Data;
using BarkodListem.Helpers;
using BarkodListem.Models;
using BarkodListem.Services;
namespace BarkodListem.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var ayarlar = await _databaseService.AyarlarGetir();
            if (ayarlar != null)
            {
                entryUrl.Text = ayarlar.WebServisURL;
                entryPort.Text = ayarlar.Port.ToString();
                entryUserId.Text = ayarlar.user_id.ToString();
                entryUser.Text = ayarlar.KullaniciAdi;
                entryPassword.Text = ayarlar.Sifre;
            }
            versionLabel.Text = "Versiyon: " + AppInfo.Current.VersionString;
            versionLabel.Text = $"Versiyon: {VersionTracking.CurrentVersion}";
        }
        public SettingsPage(DatabaseService databaseService)
        {
            InitializeComponent();
            themePicker.SelectedIndex = (ThemeHelper.SelectedTheme == AppTheme.Dark) ? 1 : 0;
            _databaseService=databaseService;
        }
        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
        private async void Kaydet_Clicked(object sender, EventArgs e)
        {
            var ayarlar = new AyarlarModel
            {
                WebServisURL = entryUrl.Text,
                Port = int.Parse(entryPort.Text),
                user_id = int.Parse(entryUserId.Text),
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
        private async void OnTemizlikClicked(object sender, EventArgs e)
        {
            await PhotoCacheService.RemoveOrphanFilesAsync();
            await PhotoCacheService.TrimUploadedCacheAsync();
            await DisplayAlert("Temizlik Tamam", "Sahipsiz dosyalar temizlendi ve fotoğraf önbelleği sınırlandı.", "Tamam");
        }
        private void Button_Pressed(object sender, EventArgs e)
        {
            if (sender is Button btn)
                btn.Opacity = 1;
        }

        private void Button_Released(object sender, EventArgs e)
        {
            if (sender is Button btn)
                btn.Opacity = 0.7;
        }
        private void Entry_Completed(object sender, EventArgs e)
        {
            if (sender is Entry entry && !string.IsNullOrEmpty(entry.Text))
            {
                entry.CursorPosition = entry.Text.Length; // 🔥 Cursor'u sona getiriyoruz
            }
        }
    }
}
