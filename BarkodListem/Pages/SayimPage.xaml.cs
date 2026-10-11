using BarkodListem.Data;
using BarkodListem.Models;
using BarkodListem.Services;
using BarkodListem.ViewModels;
using BarkodListem.Views;
using Plugin.Maui.Audio;

namespace BarkodListem.Pages;

public partial class SayimPage : ContentPage
{
    private readonly BarkodListViewModel _viewModel;
    private readonly WebService _webService;
    private readonly DatabaseService _databaseService;

    public SayimPage(BarkodListViewModel viewModel, WebService webService, DatabaseService databaseService)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _webService = webService;
        _databaseService = databaseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadData();
    }

    private async void Back_Clicked(object sender, EventArgs e) => await Navigation.PopAsync();

    [Obsolete]
    private async void QRKodTara_Clicked(object sender, EventArgs e)
    {
        var audioManager = ServiceHelper.GetService<IAudioManager>();
        await Navigation.PushAsync(new ScannerPage(_viewModel, audioManager));
    }

    private async void BarkodEkle_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(barkodEntry.Text))
            return;

        var barkod = barkodEntry.Text.Trim().ToUpperInvariant();
        barkodEntry.Text = string.Empty;

        if (barkod.StartsWith("SVK-"))
            await Navigation.PushAsync(new SevkiyatDetayPage(barkod));
        else
            await _viewModel.BarkodEkle(barkod);

        barkodEntry.Focus();
    }

    private void BarkodEntry_Completed(object sender, EventArgs e) => BarkodEkle_Clicked(sender, e);

    private async void YeniListeOlustur_Clicked(object sender, EventArgs e)
    {
        var yeniListeAdi = await DisplayPromptAsync("Yeni Liste", "Yeni liste ismi giriniz:", "Tamam", "İptal");
        if (string.IsNullOrWhiteSpace(yeniListeAdi))
            return;

        await _viewModel.YeniListeOlustur(yeniListeAdi.Trim());
        await DisplayAlert("Başarılı", $"Yeni liste oluşturuldu: {yeniListeAdi.Trim()}", "Tamam");
    }

    private async void BarkodSil_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: BarkodModel selectedBarkod })
            return;

        if (await DisplayAlert("Silme Onayı", "Seçili barkodu silmek istiyor musunuz?", "Evet", "Hayır"))
            _viewModel.BarkodSilCommand.Execute(selectedBarkod);
    }

    private async void ListeyiGonder_Clicked(object sender, EventArgs e)
    {
        var listeAdi = await DisplayPromptAsync(
            "Liste İsmi",
            "Barkodları göndermek için bir liste ismi giriniz:",
            "Tamam", "İptal", _viewModel.AktifListeAdi);

        if (string.IsNullOrWhiteSpace(listeAdi))
            listeAdi = _viewModel.AktifListeAdi;

        if (!await DisplayAlert("Gönderim Onayı", $"{listeAdi} listesini web servise göndermek istiyor musunuz?", "Evet", "Hayır"))
            return;

        var success = await _webService.BarkodListesiGonder(_viewModel.Barkodlar.ToList(), listeAdi);
        await DisplayAlert("Bilgi", success ? "Liste başarıyla gönderildi!" : "Gönderme başarısız!", "Tamam");
    }

    private async void ClearListButton_Clicked(object sender, EventArgs e)
    {
        if (await DisplayAlert("Listeyi Temizle", "Tüm kayıtları silmek istediğinizden emin misiniz?", "Evet", "Hayır"))
            await _viewModel.ClearAllBarkodsAsync();
    }

    private async void Listeler_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListelerPage(_databaseService, _viewModel, _webService));
    }

    private async void ListeAdiniDegistir_Tapped(object sender, EventArgs e)
    {
        var yeniListeAdi = await DisplayPromptAsync(
            "Liste Adı", "Yeni liste ismi giriniz:", "Tamam", "İptal", _viewModel.AktifListeAdi);

        if (!string.IsNullOrWhiteSpace(yeniListeAdi))
            await _viewModel.SetAktifListe(yeniListeAdi.Trim());
    }
}
