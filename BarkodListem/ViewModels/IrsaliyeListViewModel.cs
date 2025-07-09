using BarkodListem.Models;
using BarkodListem.Services;    // WebService namespace
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;

namespace BarkodListem.ViewModels
{
    public partial class IrsaliyeListViewModel : ObservableObject
    {
        private readonly WebService _webService;

        public IrsaliyeListViewModel(WebService webService)
        {
            _webService = webService;
            SorgulaCommand = new AsyncRelayCommand(SorgulaAsync);
        }

        [ObservableProperty]
        private DateTime startDate = DateTime.Today.AddDays(-7);

        [ObservableProperty]
        private DateTime endDate = DateTime.Today;

        [ObservableProperty]
        private bool isBusy;

        public ObservableCollection<IrsaliyeModel> Irsaliyeler { get; } = new();

        public IAsyncRelayCommand SorgulaCommand { get; }

        private async Task SorgulaAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Irsaliyeler.Clear();

                // 1️⃣ gerçek servis çağrısı
                var dt = await _webService.IrsaliyeSorgula(StartDate, EndDate);

                // 2️⃣ DataTable → ObservableCollection map’leme
                foreach (DataRow row in dt.Rows)
                {
                    Irsaliyeler.Add(new IrsaliyeModel
                    {
                        IRSALIYE_ID = Convert.ToInt32(row["IRSALIYE_ID"]),
                        IRSALIYE_TURU = row["IRSALIYE_TURU"].ToString(),
                        IRS_TARIH = Convert.ToDateTime(row["IRS_TARIH"]),
                        IRS_NO = row["IRS_NO"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                // Hata mesajını göstermek için
                await Application.Current.MainPage.DisplayAlert("Hata", ex.Message, "Tamam");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
