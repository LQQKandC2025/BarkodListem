using AndroidX.Lifecycle;
using BarkodListem.ViewModels;

namespace BarkodListem.Pages;

[QueryProperty(nameof(StokId), "stokId")]
[QueryProperty(nameof(SevkiyatNo), "sevkiyatNo")]
public partial class GaleriPage : ContentPage
{
    public string StokId { get; set; }
    public string SevkiyatNo { get; set; }

    public GaleriPage(int stokId, string sevkiyatNo)
    {
        InitializeComponent();
        var viewModel = new GaleriViewModel();
        BindingContext = viewModel;

        // ViewModel'e parametreleri geçiriyoruz
        viewModel.Yukle(stokId, sevkiyatNo);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (int.TryParse(StokId, out int stokId))
        {
            ((GaleriViewModel)BindingContext).Yukle(stokId, SevkiyatNo);
        }
    }
}
