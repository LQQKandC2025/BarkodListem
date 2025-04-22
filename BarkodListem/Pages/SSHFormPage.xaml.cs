using BarkodListem.Models;
using BarkodListem.ViewModels;

namespace BarkodListem.Pages;

public partial class SSHFormPage : ContentPage
{
    public SSHFormPage(UrunModel seciliUrun, string sevkiyatNo, string subeKodu, int sirano)
    {
        InitializeComponent();
        seciliUrun.Sirano = sirano; // 🔥 Burada modele yazıyoruz
        BindingContext = new SSHFormViewModel(seciliUrun, sevkiyatNo, subeKodu,sirano);
    }
}