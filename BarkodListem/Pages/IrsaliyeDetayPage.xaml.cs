using BarkodListem.Services;
using BarkodListem.ViewModels;
using BarkodListem.Views;

namespace BarkodListem.Pages;

public partial class IrsaliyeDetayPage : ContentPage
{
    private readonly IrsaliyeDetayViewModel _vm;

    public IrsaliyeDetayPage(int irsaliyeId)
    {
        InitializeComponent();
        var webService = ServiceHelper.GetService<WebService>();
        BindingContext = _vm = new IrsaliyeDetayViewModel(webService, irsaliyeId);

        // Sayfa açılır açılmaz veri getir:
        Loaded += async (_, __) => await _vm.SorgulaCommand.ExecuteAsync(null);

        // Barkod girişi için focus hep girişte kalsın:
        barkodEntry.Focused += (_, __) => barkodEntry.CursorPosition = barkodEntry.Text?.Length ?? 0;
    }

    private async void OnCameraClicked(object sender, EventArgs e)
    {
        // Mevcut ScannerPage’i özelleştirerek barkodu okutup tek sefer
        // barkodEntry.Text = okutulanDeğer;
        // barkodEntry.Focus();
        // örneğin:
        var result = await ScannerPage.ScanOnceAsync();
        barkodEntry.Text = result;
        barkodEntry.Focus();
    }
}
