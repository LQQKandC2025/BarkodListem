using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BarkodListem.Models;
using BarkodListem.ViewModels;

namespace BarkodListem.Pages;

public partial class IrsaliyeListPage : ContentPage
{
    private readonly IrsaliyeListViewModel viewModel;
    public IrsaliyeListPage()
    {
        InitializeComponent();
        BindingContext = viewModel = new IrsaliyeListViewModel();
    }

    private async void OnIrsaliyeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is IrsaliyeModel selected)
        {
            await DisplayAlert("İrsaliye", $"İrsaliye No: {selected.IRS_NO}", "Tamam");
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}