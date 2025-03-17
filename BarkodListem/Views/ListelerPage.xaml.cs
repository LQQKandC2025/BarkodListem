
using System.Collections.ObjectModel;
using BarkodListem.Models;
using BarkodListem.Services;
using BarkodListem.Data;
using BarkodListem.ViewModels;


namespace BarkodListem.Views
{
    public partial class ListelerPage : ContentPage
    {
        private readonly BarkodListViewModel _viewModel; // Bu artık kesinlikle atanmalı
        private readonly DatabaseService _databaseService;
        public ObservableCollection<ListeModel> Listeler { get; set; } = new();
        public ObservableCollection<BarkodModel> SeciliListeBarkodlar { get; set; } = new();


        private string dbPath = Path.Combine(FileSystem.AppDataDirectory, "barkodlistem.db");
        private readonly WebService _webService;

        public ListelerPage(DatabaseService databaseService, BarkodListViewModel viewModel, WebService webService)
        {
            InitializeComponent();
            _databaseService = databaseService;


            _viewModel = viewModel;  // ViewModel burada atandı+

            _webService=webService;
            BindingContext = this;


            LoadListeler();
        }

        private async void LoadListeler()
        {
            var kayitliListeler = await _databaseService.ListeGetir();
            foreach (var liste in kayitliListeler)
            {
                Listeler.Add(liste);
            }
        }

        private async void ListeSecildi(object sender, SelectionChangedEventArgs e)
        {
            //if (e.CurrentSelection.FirstOrDefault() is ListeModel seciliListe)
            //{
            //    SeciliListeBarkodlar.Clear();
            //    var barkodlar = await _databaseService.BarkodlariGetir(seciliListe.ListeAdi);
            //    foreach (var barkod in barkodlar)
            //    {
            //        SeciliListeBarkodlar.Add(barkod);
            //    }
            //}
            if (e.CurrentSelection.FirstOrDefault() is ListeModel seciliListe)
            {
                // Burada ListelerPage'de listelenen "seciliListe"yi ve view model'i yeni sayfaya geçiriyoruz.
                await Navigation.PushAsync(new ListeDetayPage(seciliListe, _viewModel, _webService));
            }
        }
        public async void ListeGonder_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is ListeModel liste)
            {
                string mevcutListeAdi = _viewModel.AktifListeAdi;

                // 📌 Eğer liste "Geçici Liste" ise, yeni bir isim iste
                string listeAdi = await DisplayPromptAsync("Liste İsmi",
                    "Barkodları göndermek için bir liste ismi giriniz:",
                    "Tamam", "İptal",
                    mevcutListeAdi == "Geçici Liste" ? "" : mevcutListeAdi // Geçici Liste ise boş bırak
                );

                if (string.IsNullOrEmpty(listeAdi))
                {
                    listeAdi = _viewModel.AktifListeAdi;
                }
                bool confirm = await DisplayAlert("Gönderim Onayı", $"{listeAdi} listesini web servise göndermek istiyor musunuz?", "Evet", "Hayır");
                if (confirm)
                {
                    bool success = await _webService.BarkodListesiGonder(_viewModel.Barkodlar.ToList(), listeAdi);
                    string mesaj = success ? "Liste başarıyla gönderildi!" : "Gönderme başarısız!";
                    await DisplayAlert("Bilgi", mesaj, "Tamam");
                }

            }
        }
        public async void ListeSil_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is ListeModel liste)
            {
                bool confirm = await DisplayAlert("Silme Onayı", $"{liste.ListeAdi} listesini silmek istiyor musunuz?", "Evet", "Hayır");
                if (confirm)
                {
                    await _databaseService.ListeSil(liste);
                    Listeler.Remove(liste);  // 📌 Listeden de kaldır
                }
            }
        }

    }

}

