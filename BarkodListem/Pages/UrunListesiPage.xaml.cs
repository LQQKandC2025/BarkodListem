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

            // Sevkiyat no ile �r�nleri y�kle
            _ = _viewModel.YukleAsync(sevkiyatNo);
        }

        private async void OnGeriDonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnGaleriClicked(object sender, EventArgs e)
        {
            await Application.Current.MainPage.DisplayAlert("GALER�", "Burada galeri a��lacak (devam�nda yap�lacak)", "Tamam");
        }
        private async void OnSSHButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var urun = button?.BindingContext as UrunModel;
            if (urun == null) return;

            var siraNo = _viewModel.Urunler.ToList().IndexOf(urun) + 1;
            await Navigation.PushAsync(new SSHFormPage(urun, _viewModel.GetSevkiyatNo(), urun.SUBE_KODU, siraNo));
        }
    }
}
