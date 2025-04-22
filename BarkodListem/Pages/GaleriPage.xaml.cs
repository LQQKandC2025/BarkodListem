using BarkodListem.ViewModels;

namespace BarkodListem.Pages;

[QueryProperty(nameof(StokId), "stokId")]
[QueryProperty(nameof(SevkiyatNo), "sevkiyatNo")]
public partial class GaleriPage : ContentPage
{
    public string StokId { get; set; }
    public string SevkiyatNo { get; set; }

    public GaleriPage()
    {
        InitializeComponent();
        BindingContext = new GaleriViewModel();
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
