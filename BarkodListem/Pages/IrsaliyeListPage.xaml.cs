using BarkodListem.Models;
using BarkodListem.Data;
using BarkodListem.Services;
using BarkodListem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarkodListem.Pages;

public partial class IrsaliyeListPage : ContentPage
{
   
    private readonly IrsaliyeListViewModel _viewModel;
    public IrsaliyeListPage()
    {
        InitializeComponent();

        // DatabaseService ve WebService örneklerini oluşturun
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "barkodlistem.db");
        var dbService = new DatabaseService(dbPath);
        var webService = new WebService(dbService);

        // ViewModel’i buna geçir
        BindingContext = _viewModel = new IrsaliyeListViewModel(webService);
    }

    private async void OnIrsaliyeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is IrsaliyeModel selected)
        {
             await DisplayAlert("İrsaliye", $"İrsaliye No: {selected.IRS_NO}", "Tamam");
            ((CollectionView)sender).SelectedItem = null;
        }
    }
    private async void OnIrsaliyeTapped(object sender, TappedEventArgs e)
    {
        // sender Frame olduğu için onun BindingContext’inden modeli alıyoruz
        if (sender is Frame frame && frame.BindingContext is IrsaliyeModel secilen)
        {
            await DisplayAlert("İrsaliye", $"İrsaliye No: {secilen.IRS_NO}", "Tamam");
        }
    }
}