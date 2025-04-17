using BarkodListem.Models;
using BarkodListem.ViewModels;

namespace BarkodListem.Pages
{
    public partial class UrunListesiPage : ContentPage
    {
        private readonly UrunListesiViewModel _viewModel;

        public UrunListesiPage(string sevkiyatNo)
        {
            InitializeComponent();
            _viewModel = new UrunListesiViewModel();
            BindingContext = _viewModel;

            // Sevkiyat no ile ürünleri yükle
            _ = _viewModel.YukleAsync(sevkiyatNo);
        }

        private async void OnGeriDonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnGaleriClicked(object sender, EventArgs e)
        {
            await Application.Current.MainPage.DisplayAlert("GALERÝ", "Burada galeri açýlacak (devamýnda yapýlacak)", "Tamam");
        }
        private async void OnSSHButtonClicked(object sender, EventArgs e)
        {
            // Butona týklanan satýrdaki ürünü al
            var button = sender as Button;
            var urun = button?.BindingContext as UrunModel;

            if (urun == null)
                return;

            // SSH formuna gerekli parametreleri taþý
            await Navigation.PushAsync(new SSHFormPage(urun));
        }
    }
}
