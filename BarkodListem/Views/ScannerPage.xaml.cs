using BarkodListem.ViewModels;
using BarkodListem.Views
namespace BarkodListem.Views
{
	public partial class ScannerPage : ContentPage
	{
		private readonly BarkodListViewModel viewModel;

		public ScannerPage(BarkodListViewModel vm)
		{
			InitializeComponent();
			viewModel = vm;
			BindingContext = viewModel;
		}

		private void OnBarcodeDetected(object sender, ZXing.Net.MAUI.BarcodeDetectionEventArgs e)
		{
			if (e.Results.Any())
			{
				var barkod = e.Results[0].Value;
				viewModel.BarkodEkleCommand.Execute(barkod);
				DisplayAlert("Barkod Okundu", barkod, "Tamam");
			}
		}
	}
}
