
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using BarkodListem.Models;
using BarkodListem.Services;
using BarkodListem.Data;
using BarkodListem.ViewModels;


namespace BarkodListem.Views
{
    public partial class ListelerPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        public ObservableCollection<ListeModel> Listeler { get; set; } = new();
        public ObservableCollection<BarkodModel> SeciliListeBarkodlar { get; set; } = new();

        private readonly BarkodListViewModel _viewModel;
        private string dbPath = Path.Combine(FileSystem.AppDataDirectory, "barkodlistem.db");
        public ListelerPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
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
            if (e.CurrentSelection.FirstOrDefault() is ListeModel seciliListe)
            {
                SeciliListeBarkodlar.Clear();
                var barkodlar = await _databaseService.BarkodlariGetir(seciliListe.ListeAdi);
                foreach (var barkod in barkodlar)
                {
                    SeciliListeBarkodlar.Add(barkod);
                }
            }
        }
        public async void ListeGonder_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is ListeModel liste)
            {
                bool confirm = await DisplayAlert("Gönderim Onayı", $"{liste.ListeAdi} listesini web servise göndermek istiyor musunuz?", "Evet", "Hayır");
                if (confirm)
                {
                    var webService = new WebService();
                    bool success = await webService.BarkodListesiGonder(liste.Barkodlar.ToList(), liste.ListeAdi);
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

