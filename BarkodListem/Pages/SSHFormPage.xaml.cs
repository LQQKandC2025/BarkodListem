using BarkodListem.Models;
using BarkodListem.ViewModels;

namespace BarkodListem.Pages;

public partial class SSHFormPage : ContentPage
{
    public SSHFormPage(UrunModel urun, string sevkiyatNo, string subeKodu, int siraNo)
    {
        InitializeComponent();
        BindingContext = new SSHFormViewModel(urun, sevkiyatNo, subeKodu, siraNo);
    }
}