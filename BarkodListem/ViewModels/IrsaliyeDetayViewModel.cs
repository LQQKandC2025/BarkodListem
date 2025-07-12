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
    public partial class IrsaliyeDetayViewModel : ObservableObject
    {
        private readonly WebService _webService;
        private readonly int _irsaliyeId;

        [ObservableProperty]
        private bool isBusy;

        public ObservableCollection<PaketHrkModel> PaketHareketler { get; } = new();

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
                foreach (DataRow row in dt.Rows)
                {
                    PaketHareketler.Add(new PaketHrkModel
                    {
                        PAKET_HRK_ID = Convert.ToInt32(row["PAKET_HRK_ID"]),
                        PAKET_ADI = row["PAKET_ADI"].ToString(),
                        KAREKOD = row["KAREKOD"].ToString(),
                        TERMINAL = row["TERMINAL"].ToString(),
                        ZR_KAREKOD = row["ZR_KAREKOD"].ToString(),
                        KAREKOD_ID = Convert.ToInt32(row["KAREKOD_ID"])
                    });
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Gelen barkoda göre satırları vurgula
        public void HighlightByBarcode(string barcode)
        {
            foreach (var item in PaketHareketler) {
            item.IsHighlighted = item.KAREKOD == barcode;
            if(item.KAREKOD == barcode)
            {
                    item.IsHighlighted = true;
            }
            }
        }
    }
}
