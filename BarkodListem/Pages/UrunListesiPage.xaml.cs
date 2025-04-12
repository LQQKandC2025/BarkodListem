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
    }
}
