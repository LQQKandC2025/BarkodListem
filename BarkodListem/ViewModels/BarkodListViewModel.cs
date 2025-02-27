﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using BarkodListem.Data;
using BarkodListem.Models;
using BarkodListem.Services;

namespace BarkodListem.ViewModels
{
    public class BarkodListViewModel : BindableObject
    {

        private readonly DatabaseService _databaseService;
        private readonly WebService _webService;

        private string _aktifListeAdi;
        public string AktifListeAdi
        {
            get => _aktifListeAdi;
            set
            {
                _aktifListeAdi = value;
                OnPropertyChanged(nameof(AktifListeAdi));
            }
        }

        public ObservableCollection<BarkodModel> Barkodlar { get; set; } = new ObservableCollection<BarkodModel>();

        public ICommand BarkodEkleCommand { get; }
        public ICommand BarkodSilCommand { get; }
        public ICommand ListeyiGonderCommand { get; }
        public async Task LoadData()
        {

            // 📌 Program açıldığında liste boş gelsin
            Barkodlar.Clear();
        }

        [Obsolete]
        public BarkodListViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _webService = new WebService();
            BarkodEkleCommand = new Command<string>(async (barkod) => await BarkodEkle(barkod));
            BarkodSilCommand = new Command<BarkodModel>(async (barkod) => await BarkodSil(barkod));
            ListeyiGonderCommand = new Command(async () => await ListeyiGonder());
            // 📌 Varsayılan liste adı
            AktifListeAdi = "Geçici Liste";
        }
        public async Task BarkodEkle(string barkod)
        {
            if (!string.IsNullOrEmpty(barkod))
            {
                // Eğer ilk defa ekleniyorsa liste ismi sorulsun
                if (string.IsNullOrEmpty(_aktifListeAdi))
                {
                    string girilenListe = await Application.Current.MainPage.DisplayPromptAsync(
                        "Liste İsmi", "Lütfen liste ismi giriniz:", "Tamam", "İptal", "Liste İsmi");
                    if (string.IsNullOrEmpty(girilenListe))
                        return; // Kullanıcı iptal ederse işlem iptal
                    _aktifListeAdi = girilenListe;
                }

                var yeniBarkod = new BarkodModel { Barkod = barkod, ListeAdi = _aktifListeAdi };
                await _databaseService.BarkodEkle(yeniBarkod);
                Barkodlar.Insert(0, yeniBarkod);  // Yeni barkod en üste eklensin

                // Liste kaydırma: Yeni eklenen barkod görünür olsun
                await Task.Delay(500);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    int index = Barkodlar.IndexOf(yeniBarkod);
                    if (index != -1)
                    {
                        MainPage.Instance.BarkodListesi.ScrollTo(index, position: ScrollToPosition.Start, animate: true);
                    }
                    else
                    {
                        Console.WriteLine("Hata: Barkod listede yok, kaydırma yapılamadı.");
                    }
                });
            }
        }

        private async Task BarkodSil(BarkodModel barkod)
        {
            if (barkod != null)
            {
                await _databaseService.BarkodSil(barkod);

                var itemToRemove = Barkodlar.FirstOrDefault(b => b.Barkod == barkod.Barkod);
                if (itemToRemove != null)
                {
                    Barkodlar.Remove(itemToRemove);
                }
            }
        }

        [Obsolete]
        private async Task ListeyiGonder()
        {
            if (Barkodlar.Count == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", "Gönderilecek barkod yok!", "Tamam");
                return;
            }

            // 📌 Sadece ilk defa liste ismi sorulsun
            if (string.IsNullOrEmpty(_aktifListeAdi))
            {
                _aktifListeAdi = await Application.Current.MainPage.DisplayPromptAsync(
                    "Liste İsmi", "Lütfen liste ismi giriniz:", "Tamam", "İptal", "Liste İsmi");

                if (string.IsNullOrEmpty(_aktifListeAdi)) return; // Kullanıcı iptal ettiyse gönderme
            }

            bool success = await _webService.BarkodListesiGonder(Barkodlar.ToList(), _aktifListeAdi);

            string mesaj = success ? "Liste başarıyla gönderildi!" : "Gönderme başarısız!";
            await Application.Current.MainPage.DisplayAlert("Bilgi", mesaj, "Tamam");
        }
        public async Task ClearAllBarkodsAsync()
        {
            bool dbResult = await DatabaseService.DeleteAllBarkodsAsync(); // SQLite'da tüm kayıtları silen metot
            if (dbResult)
            {
                // UI listesini de temizle
                Barkodlar.Clear();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Hata", "Kayıtlar silinemedi.", "Tamam");
            }
        }
        public void SetAktifListe(string yeniListeAdi)
        {
            _aktifListeAdi = yeniListeAdi;
            Barkodlar.Clear();  // 📌 Liste değiştiği için barkodları temizle
        }

    }
}
