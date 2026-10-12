using BarkodListem.Models;
using BarkodListem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;

namespace BarkodListem.ViewModels
{
    public enum BarkodOkutmaSonucu
    {
        Bulunamadi,
        ZatenOkutulmus,
        Okutuldu
    }

    public partial class IrsaliyeDetayViewModel : ObservableObject
    {
        private readonly WebService _webService;
        private readonly int _irsaliyeId;

        [ObservableProperty]
        private bool isBusy;

        public ObservableCollection<PaketHrkModel> PaketHareketler { get; } = new();

        public int IrsaliyeId => _irsaliyeId;

        public IAsyncRelayCommand SorgulaCommand { get; }

        public IrsaliyeDetayViewModel(WebService webService, int irsaliyeId)
        {
            _webService = webService;
            _irsaliyeId = irsaliyeId;
            SorgulaCommand = new AsyncRelayCommand(SorgulaAsync);
        }

        private async Task SorgulaAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                PaketHareketler.Clear();

                var dt = await _webService.IrsaliyeDetaySorgula(_irsaliyeId);
                if (dt.Rows.Count == 0)
                {
                    await App.Current.MainPage.DisplayAlert("Bilgi", "İrsaliye detayında kayıt bulunamadı.", "Tamam");
                    return;
                }

                foreach (DataRow row in dt.Rows)
                {
                    var terminalText = row["TERMINAL"]?.ToString() ?? string.Empty;
                    PaketHareketler.Add(new PaketHrkModel
                    {
                        PAKET_HRK_ID = Convert.ToInt32(row["PAKET_HRK_ID"]),
                        PAKET_ADI = row["PAKET_ADI"].ToString(),
                        KAREKOD = row["KAREKOD"].ToString(),
                        TERMINAL = row["TERMINAL"].ToString(),
                        ZR_KAREKOD = row["ZR_KAREKOD"].ToString(),
                        KAREKOD_ID = Convert.ToInt32(row["KAREKOD_ID"]),
                        IsHighlighted = !string.IsNullOrWhiteSpace(terminalText)
                    });
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Gelen barkoda göre yalnızca eşleşen satırı vurgular; önceki okutmalar korunur.
        public BarkodOkutmaSonucu HighlightByBarcode(string barcode)
        {
            string normalizedBarcode = (barcode ?? string.Empty).Trim();
            var matches = PaketHareketler
                .Where(item => string.Equals(item.KAREKOD?.Trim(), normalizedBarcode,
                                             StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 0)
                return BarkodOkutmaSonucu.Bulunamadi;

            bool yeniOkutmaVar = false;
            foreach (var item in matches)
            {
                if (!item.IsHighlighted)
                {
                    item.IsHighlighted = true;
                    item.IsScannedThisSession = true;
                    yeniOkutmaVar = true;
                }
            }

            return yeniOkutmaVar
                ? BarkodOkutmaSonucu.Okutuldu
                : BarkodOkutmaSonucu.ZatenOkutulmus;
            }
        }
    }
