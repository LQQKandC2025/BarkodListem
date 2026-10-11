using BarkodListem.ViewModels;
namespace BarkodListem.Pages;
[QueryProperty(nameof(StokId), "stokId")]
[QueryProperty(nameof(SevkiyatNo), "sevkiyatNo")]
public partial class TFGaleriPage : ContentPage
{
    public string StokId { get; set; }
    public string SevkiyatNo { get; set; }
    public TFGaleriPage(int stokId, string sevkiyatNo)
    {
        InitializeComponent();
        var viewModel = new GaleriViewModel();
        BindingContext = viewModel;
        StokId = stokId.ToString();
        SevkiyatNo = sevkiyatNo;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (int.TryParse(StokId, out int stokId))
        {
            ((GaleriViewModel)BindingContext).Yukle(stokId, SevkiyatNo, "TESLİMAT");
        }
    }
    private async void Back_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
