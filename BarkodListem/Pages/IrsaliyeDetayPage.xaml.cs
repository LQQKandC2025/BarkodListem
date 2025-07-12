using AndroidX.Lifecycle;
using BarkodListem.Helpers;
using BarkodListem.Services;
using BarkodListem.ViewModels;
using BarkodListem.Views;

namespace BarkodListem.Pages
{
    public partial class IrsaliyeDetayPage : ContentPage
    {
        readonly IrsaliyeDetayViewModel _vm;
        readonly WebService _webService;
        public IrsaliyeDetayPage(int irsaliyeId)
        {
            InitializeComponent();

            _webService = ServiceHelper.GetService<WebService>();
            _vm = new IrsaliyeDetayViewModel(_webService, irsaliyeId);

            BindingContext = _vm;

            Loaded += async (_, __) => await _vm.SorgulaCommand.ExecuteAsync(null);

            barkodEntry.Focused += (_, __) =>
                barkodEntry.CursorPosition = barkodEntry.Text?.Length ?? 0;
        }

        private async void OnCameraClicked(object sender, EventArgs e)
        {
            var result = await ScannerPage.ScanOnceAsync();
            barkodEntry.Text = result;
            ProcessBarcode(result);
        }

        private void OnEntryCompleted(object sender, EventArgs e)
        {
            var code = barkodEntry.Text?.Trim();
            if (!string.IsNullOrEmpty(code))
                ProcessBarcode(code);
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = e.NewTextValue;
            if (string.IsNullOrEmpty(text)) return;
            if (text.EndsWith("\r") || text.EndsWith("\n"))
            {
                var code = text.Trim('\r', '\n');
                barkodEntry.Text = code;
                ProcessBarcode(code);
            }
        }

        void ProcessBarcode(string code)
        {
            _vm.HighlightByBarcode(code);
            barkodEntry.Focus();
            barkodEntry.Text = String.Empty;
            barkodEntry.CursorPosition = barkodEntry.Text?.Length ?? 0;
        }

        private async void OnGonderClicked(object sender, EventArgs e)
        {
            var allItems = _vm.PaketHareketler;
            var highlightedIds = allItems
                .Where(x => x.IsHighlighted)
                .Select(x => x.PAKET_HRK_ID)
                .ToArray();

            int notHighlightedCount = allItems.Count - highlightedIds.Length;
            if (notHighlightedCount > 0)
            {
                // Eksik ürün var: onay iste
                bool proceed = await DisplayAlert(
                    "Uyarı",
                    $"{notHighlightedCount} adet ürünün barkodu okutulmadı. Yine de kaydetmek istiyor musunuz?",
                    "Evet",
                    "İptal"
                );
                if (!proceed)
                    return;
            }

            if (highlightedIds.Length == 0)
            {
                await DisplayAlert("Uyarı", "Güncellenecek satır yok.", "Tamam");
                return;
            }

            // Toplu güncelleme çağrısı
            int updatedCount = await _webService.IrsaliyeDetayGuncelleAsync(
                SettingsHelper.UserName,
                SettingsHelper.Password,
                highlightedIds,
                AppGlobals.mobil_id.ToString()
            );
            await DisplayAlert("Bilgi", $"{updatedCount} satır işlendi.", "Tamam");
        }

    }
}
