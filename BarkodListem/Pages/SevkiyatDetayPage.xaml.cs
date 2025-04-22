using BarkodListem.ViewModels;
using BarkodListem.Pages;
using BarkodListem.Models;
namespace BarkodListem.Pages
{
    public partial class SevkiyatDetayPage : ContentPage
    {
        private readonly SevkiyatDetayViewModel viewModel;
        public string MOBIL_PERSONEL { get; set; } = AppGlobals.mobil_id.ToString();
        public SevkiyatDetayPage(string sevkiyatNo)
        {
            InitializeComponent();
            BindingContext = viewModel = new SevkiyatDetayViewModel(sevkiyatNo);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await viewModel.LoadSevkiyatAsync(); // Web servisten verileri �ekecek
        }
        private async void OnUrunListesiClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(viewModel.SEVKIYAT_NO))
            {
                await Navigation.PushAsync(new UrunListesiPage(viewModel.SEVKIYAT_NO));
            }
            else
            {
                await DisplayAlert("Hata", "Sevkiyat numaras� bo�!", "Tamam");
            }
        }
    }
}


