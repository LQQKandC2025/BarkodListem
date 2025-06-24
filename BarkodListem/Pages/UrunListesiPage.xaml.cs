using BarkodListem.Models;
using BarkodListem.ViewModels;
namespace BarkodListem.Pages
{
    public partial class UrunListesiPage : ContentPage
    {
        private readonly UrunListesiViewModel _viewModel;

        private const int LongPressThreshold = 800; // 800ms
        public UrunListesiPage(string sevkiyatNo)
        {
            InitializeComponent();
            _viewModel = new UrunListesiViewModel();
            BindingContext = _viewModel;
            _ = _viewModel.YukleAsync(sevkiyatNo);
        }
        private async void OnGeriDonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
        private async void OnGaleriClicked(object sender, EventArgs e)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new TFGaleriPage(0, AppGlobals.AktifSevkiyat));
        }
        private async void OnSSHButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is UrunModel urun && urun != null)
            {
                UrunBilgi.STOK_ADI = urun.STOK_ADI;
                UrunBilgi.STOK_SPEC = urun.STOK_SPEC;
                UrunBilgi.SEVK_ARAC_ID = urun.SEVK_ARAC_ID;
                UrunBilgi.SIP_STR_ID = urun.SIP_STR_ID;
                UrunBilgi.SEVK_FIS_ID = urun.SEVK_FIS_ID;
                UrunBilgi.STOK_ID = urun.STOK_ID;
                UrunBilgi.SUBE_KODU = urun.SUBE_KODU;
                UrunBilgi.CARI_ID = urun.CARI_ID;
                UrunBilgi.SEVKIYAT_NO = urun.SEVKIYAT_NO;
                var siraNo = _viewModel.Urunler.ToList().IndexOf(urun) + 1;
                await Navigation.PushAsync(new SSHFormPage(urun, _viewModel.GetSevkiyatNo(), urun.SUBE_KODU, siraNo));
            }
        }
        public void RefreshCollectionView()
        {
            var eskiUrunler = _viewModel.Urunler.ToList(); // Mevcutlarý kopyala
            _viewModel.Urunler.Clear();                    // Listeyi boþalt
            foreach (var urun in eskiUrunler)
                _viewModel.Urunler.Add(urun);              // Tek tek geri ekle
        }

    }
}
