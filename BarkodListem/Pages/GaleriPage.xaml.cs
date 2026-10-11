using BarkodListem.ViewModels;
namespace BarkodListem.Pages;
[QueryProperty(nameof(StokId), "stokId")]
[QueryProperty(nameof(SevkiyatNo), "sevkiyatNo")]
public partial class GaleriPage : ContentPage
{
    public string StokId { get; set; }
    public string SevkiyatNo { get; set; }
    public string SipStrId { get; set; }
    public GaleriPage(int stokId, string sevkiyatNo, int sipStrId)
    {
        InitializeComponent();
        var viewModel = new GaleriViewModel();
        BindingContext = viewModel;
        StokId = stokId.ToString();
        SevkiyatNo = sevkiyatNo;
        SipStrId = sipStrId.ToString();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (int.TryParse(StokId, out int stokId) && int.TryParse(SipStrId, out int sipStrId))
        {
            ((GaleriViewModel)BindingContext).Yukle(stokId, SevkiyatNo, "SSH", sipStrId);
        }
    }
    private async void Back_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
