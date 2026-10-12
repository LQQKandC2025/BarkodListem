using AndroidX.Lifecycle;
using BarkodListem.Helpers;
using BarkodListem.Services;
using BarkodListem.ViewModels;
using BarkodListem.Views;
using BarkodListem.Models;
using System.Windows.Input;

namespace BarkodListem.Pages
{
    public partial class IrsaliyeDetayPage : ContentPage
    {
        readonly IrsaliyeDetayViewModel _vm;
        readonly WebService _webService;
        public ICommand PaketUzunBasCommand { get; }

        public IrsaliyeDetayPage(int irsaliyeId)
        {
            PaketUzunBasCommand = new Command<PaketHrkModel>(async item => await OnPaketUzunBasAsync(item));
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
            if (string.IsNullOrWhiteSpace(code))
                return;

            var sonuc = _vm.HighlightByBarcode(code);
            scanStatusLabel.Text = sonuc switch
            {
                BarkodOkutmaSonucu.Okutuldu => "✓ Paket eşleşti",
                BarkodOkutmaSonucu.ZatenOkutulmus => "• Paket zaten okutulmuş",
                _ => "✕ Bu barkod bu irsaliyede yok"
            };
            scanStatusLabel.TextColor = sonuc switch
            {
                BarkodOkutmaSonucu.Okutuldu => Color.FromArgb("#176B56"),
                BarkodOkutmaSonucu.ZatenOkutulmus => Color.FromArgb("#B36B20"),
                _ => Color.FromArgb("#B33A46")
            };
            barkodEntry.Focus();
            barkodEntry.Text = String.Empty;
            barkodEntry.CursorPosition = barkodEntry.Text?.Length ?? 0;
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnGonderClicked(object sender, EventArgs e)
        {
            var allItems = _vm.PaketHareketler;
            var newlyScannedIds = allItems
                .Where(x => x.IsScannedThisSession)
                .Select(x => x.PAKET_HRK_ID)
                .ToArray();

            int notHighlightedCount = allItems.Count(x => !x.IsHighlighted);
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

            if (newlyScannedIds.Length == 0)
            {
                await DisplayAlert("Bilgi", "Bu oturumda kaydedilecek yeni okutma yok.", "Tamam");
                return;
            }

            try
            {
                int updatedCount = await _webService.IrsaliyeDetayGuncelleGuvenliAsync(
                    SettingsHelper.UserName,
                    SettingsHelper.Password,
                    _vm.IrsaliyeId,
                    newlyScannedIds,
                    AppGlobals.mobil_id.ToString()
                );

                if (updatedCount != newlyScannedIds.Length)
                {
                    await DisplayAlert("Kayıt kontrolü",
                        $"{newlyScannedIds.Length} yeni okutma gönderildi, {updatedCount} satır kaydedildi. " +
                        "Kaydedilmeyen satırlar yeniden denenebilir.", "Tamam");
                    return;
                }

                foreach (var item in allItems.Where(x => x.IsScannedThisSession))
                    item.IsScannedThisSession = false;

                scanStatusLabel.Text = $"✓ {updatedCount} yeni paket kaydedildi";
                scanStatusLabel.TextColor = Color.FromArgb("#176B56");
                await DisplayAlert("Bilgi", $"{updatedCount} yeni paket kaydedildi.", "Tamam");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Kayıt hatası", ex.Message, "Tamam");
            }
        }

        private async void OnResetAllClicked(object sender, EventArgs e)
        {
            var scannedItems = _vm.PaketHareketler.Where(x => x.IsHighlighted).ToList();
            if (scannedItems.Count == 0)
            {
                await DisplayAlert("Bilgi", "Sıfırlanacak okutulmuş paket yok.", "Tamam");
                return;
            }

            bool confirm = await DisplayAlert(
                "Tüm okutmaları sıfırla",
                $"Bu irsaliyedeki {scannedItems.Count} okutma kaydı geri alınacak. Paketler tekrar okutulabilir. Devam edilsin mi?",
                "Sıfırla", "İptal");
            if (!confirm) return;

            await ServerKayitlariniGeriAlAsync(scannedItems);
        }

        private async Task OnPaketUzunBasAsync(PaketHrkModel? item)
        {
            if (item is null) return;

            string action = await DisplayActionSheet(
                item.PAKET_ADI ?? "Paket",
                "İptal",
                null,
                "Bu paketi okutulmamış yap");
            if (action != "Bu paketi okutulmamış yap") return;

            if (!item.IsHighlighted)
            {
                await DisplayAlert("Bilgi", "Bu paket henüz okutulmuş görünmüyor.", "Tamam");
                return;
            }

            bool confirm = await DisplayAlert(
                "Okutmayı geri al",
                "Bu paketin kaydedilmiş okutma bilgisi silinecek ve paket yeniden okutulabilir olacak.",
                "Geri al", "İptal");
            if (!confirm) return;

            await ServerKayitlariniGeriAlAsync(new[] { item });
        }

        private async Task ServerKayitlariniGeriAlAsync(IEnumerable<PaketHrkModel> items)
        {
            var selectedItems = items.DistinctBy(x => x.PAKET_HRK_ID).ToList();
            try
            {
                int updatedCount = await _webService.IrsaliyeDetayOkutmaIptalGuvenliAsync(
                    _vm.IrsaliyeId,
                    selectedItems.Select(x => x.PAKET_HRK_ID).ToArray(),
                    AppGlobals.mobil_id.ToString());

                if (updatedCount != selectedItems.Count)
                {
                    await DisplayAlert("Sıfırlama kontrolü",
                        $"{selectedItems.Count} paket için işlem istendi, {updatedCount} kayıt geri alındı. " +
                        "Değişmeyen kayıtlar korunmuştur.", "Tamam");
                    return;
                }

                foreach (var item in selectedItems)
                {
                    item.IsHighlighted = false;
                    item.IsScannedThisSession = false;
                    item.TERMINAL = string.Empty;
                }

                scanStatusLabel.Text = $"✓ {updatedCount} okutma geri alındı";
                scanStatusLabel.TextColor = Color.FromArgb("#176B56");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Sıfırlama hatası", ex.Message, "Tamam");
            }
        }

    }
}
