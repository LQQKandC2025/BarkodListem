using System.Collections.ObjectModel;
using System.Windows.Input;
using $safeprojectname$.Data;
using $safeprojectname$.Models;
using $safeprojectname$.Services;
namespace $safeprojectname$.ViewModels
{
    public class BarkodListViewModel : BindableObject
    {
        private readonly DatabaseService _databaseService;
        public ObservableCollection<BarkodModel> Barkodlar { get; set; } = new();
        private readonly WebService _webService;

        public ICommand BarkodEkleCommand { get; }
        public ICommand BarkodSilCommand { get; }
        public ICommand ListeyiGonderCommand { get; }
        private async Task LoadData()
        {
            var kayitliBarkodlar = await _databaseService.BarkodlariGetir();
            foreach (var item in kayitliBarkodlar)
            {
                Barkodlar.Add(item);
            }
        }

        [Obsolete]
        public BarkodListViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _webService = new WebService();
            BarkodEkleCommand = new Command<string>(async (barkod) => await BarkodEkle(barkod));
            BarkodSilCommand = new Command<BarkodModel>(async (barkod) => await BarkodSil(barkod));
            ListeyiGonderCommand = new Command(async () => await ListeyiGonder());
            Task.Run(async () => await LoadData());
        }


        private async Task BarkodEkle(string barkod)
        {
            if (!string.IsNullOrEmpty(barkod))
            {
                var yeniBarkod = new BarkodModel { Barkod = barkod };
                await _databaseService.BarkodEkle(yeniBarkod);
                Barkodlar.Add(yeniBarkod);
            }
        }

        private async Task BarkodSil(BarkodModel barkod)
        {
            if (barkod != null)
            {
                await _databaseService.BarkodSil(barkod);
                Barkodlar.Remove(barkod);
            }
        }

        [Obsolete]
        private async Task ListeyiGonder()
        {
            if (Application.Current?.MainPage != null)
            {
                bool success = await _webService.BarkodListesiGonder(Barkodlar.ToList());
                string mesaj = success ? "Liste başarıyla gönderildi!" : "Gönderme başarısız!";
                await Application.Current.MainPage.DisplayAlert("Bilgi", mesaj, "Tamam");
            }
        }
    }
}
