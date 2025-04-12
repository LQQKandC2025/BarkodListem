using BarkodListem.Models;
using BarkodListem.Services;
using BarkodListem.ViewModels;

namespace BarkodListem.Views
{
    public partial class ListeDetayPage : ContentPage
    {
        // Bu sayfada, ListelerPage'den gönderilen seçili liste olacak.
        public ListeModel Liste { get; set; }
        private readonly BarkodListViewModel _viewModel;
        private readonly WebService _webService;

        public ListeDetayPage(ListeModel liste, BarkodListViewModel viewModel, WebService webService)
        {
            InitializeComponent();
            Liste = liste;
            _viewModel = viewModel;
            BindingContext = this;
            _webService=webService;
        }

        // "Düzenle" butonu: Seçili listenin barkodlarını ana ekrana aktarmak
        private async void Duzenle_Clicked(object sender, EventArgs e)
        {
            // _viewModel'nin null olmadığından ve Liste (seçili liste) dolu olduğundan emin olun
            if (_viewModel != null && Liste != null)
            {
                await _viewModel.SetAktifListe(Liste.ListeAdi);
                // Ana sayfaya geri dön (örneğin Navigation.PopAsync() veya PopToRootAsync())
                await Navigation.PopAsync();
            }
        }

        // "Gönder" butonu: Seçili listeyi web servise gönder
        private async void Gonder_Clicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Gönderim Onayı", $"{Liste.ListeAdi} listesini web servise göndermek istiyor musunuz?", "Evet", "Hayır");
            if (confirm)
            {

                bool success = await _webService.BarkodListesiGonder(Liste.Barkodlar.ToList(), Liste.ListeAdi);
                string mesaj = success ? "Liste başarıyla gönderildi!" : "Gönderme başarısız!";
                await DisplayAlert("Bilgi", mesaj, "Tamam");
            }
        }

        // "Sil" butonu: Seçili listeyi ve içindeki barkodları sil
        private async void Sil_Clicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Silme Onayı", $"{Liste.ListeAdi} listesini silmek istiyor musunuz?", "Evet", "Hayır");
            if (confirm)
            {
                await _viewModel.ClearAllBarkodsAsync(); // Veritabanında o listeye ait barkodları silmek için _databaseService.ListeSil(liste) çağrılabilir
                await DisplayAlert("Bilgi", "Liste silindi.", "Tamam");
                await Navigation.PopAsync();
            }
        }
    }
}
