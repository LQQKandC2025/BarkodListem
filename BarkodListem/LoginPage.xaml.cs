using BarkodListem.Data;
using BarkodListem.Models;
namespace BarkodListem;
public partial class LoginPage : ContentPage
{
    public string dbPath = Path.Combine(FileSystem.AppDataDirectory, "barkodlistem.db");
    public DatabaseService _databaseService;
    public LoginPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService(dbPath);
        CheckDatabaseForCredentials();
    }
    [System.Obsolete]
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = usernameEntry.Text?.Trim();
        string password = passwordEntry.Text?.Trim();
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            errorLabel.Text = "Kullanıcı adı ve şifre boş olamaz!";
            errorLabel.IsVisible = true;
            return;
        }
        if (await ValidateCredentials(username, password))
        {
            App.LoginSuccessful(); // Giriş başarılıysa MainPage aç
        }
        else
        {
            errorLabel.Text = "Hatalı kullanıcı adı veya şifre!";
            errorLabel.IsVisible = true;
        }
    }
    private async Task<bool> ValidateCredentials(string username, string password)
    {
        var ayarlar = await _databaseService.AyarlarGetir();
        if (ayarlar != null)
        {
            AppGlobals.mobil_id = ayarlar.user_id;
            return username == ayarlar.KullaniciAdi && password == ayarlar.Sifre;
        }
        return false;
    }
    private async void CheckDatabaseForCredentials()
    {
        var ayarlar = await _databaseService.AyarlarGetir();
        if (ayarlar == null)
        {
            await _databaseService.AyarKaydet(new AyarlarModel
            {
                KullaniciAdi = "1",
                Sifre = "1",
                WebServisURL = "http://91.93.49.85/BarkodWebService",
                Port = 8080,
                user_id = 1
            });
        }
    }
}
