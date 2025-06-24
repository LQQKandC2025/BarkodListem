using BarkodListem.ViewModels;
using BarkodListem.Views;
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
            await viewModel.LoadSevkiyatAsync();
            var sevkvm = (SevkiyatDetayViewModel)this.BindingContext;
            AppGlobals.teslimnotu= sevkvm.TESLIM_NOTU;
            var navigation = Application.Current.MainPage.Navigation;

            // 🔍 Stack'te ilk bulduğu ScannerPage örneğini bul
            var scannerPage = navigation.NavigationStack.FirstOrDefault(p => p is ScannerPage);

            if (scannerPage != null)
            {
                navigation.RemovePage(scannerPage);
            }
        }
        private async void OnUrunListesiClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(viewModel.SEVKIYAT_NO))
            {
                await Navigation.PushAsync(new UrunListesiPage(viewModel.SEVKIYAT_NO));
            }
            else
            {
                await DisplayAlert("Hata", "Sevkiyat numarası boş!", "Tamam");
            }
        }
    }
}
