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
    private bool _irsaliyePrefixDuzenleniyor;
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

    private async void Back_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void IrsaliyeSorgula_Clicked(object sender, EventArgs e)
    {
        // Liste, yazılım klavyesinin altında kalmasın: sorgu başlamadan odağı bırak.
        irsaliyeNoEntry.Unfocus();
        await Task.Yield();

        if (_viewModel.SorgulaCommand.CanExecute(null))
            await _viewModel.SorgulaCommand.ExecuteAsync(null);
    }

    private void IrsaliyeNoEntry_Focused(object sender, FocusEventArgs e)
    {
        IrsaliyePrefixiniUygula();
    }

    private void IrsaliyeNoEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (irsaliyeNoEntry.IsFocused)
            IrsaliyePrefixiniUygula();
    }

    private void IrsaliyePrefixiniUygula()
    {
        if (_irsaliyePrefixDuzenleniyor)
            return;

        const string prefix = "IRS-";
        string mevcut = irsaliyeNoEntry.Text ?? string.Empty;
        string duzeltilmis = PrefixiUygula(mevcut, prefix);
        if (mevcut == duzeltilmis)
            return;

        _irsaliyePrefixDuzenleniyor = true;
        irsaliyeNoEntry.Text = duzeltilmis;
        irsaliyeNoEntry.CursorPosition = duzeltilmis.Length;
        _irsaliyePrefixDuzenleniyor = false;
    }

    private static string PrefixiUygula(string value, string prefix)
    {
        string code = value.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(code) || prefix.StartsWith(code, StringComparison.OrdinalIgnoreCase))
            return prefix;
        if (code.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return prefix + code[prefix.Length..];
        return prefix + code.TrimStart('-');
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
        if (sender is BindableObject element &&
            element.BindingContext is IrsaliyeModel secilen)
        {
            await Navigation.PushAsync(
                new IrsaliyeDetayPage(secilen.IRSALIYE_ID));
        }
    }
}
