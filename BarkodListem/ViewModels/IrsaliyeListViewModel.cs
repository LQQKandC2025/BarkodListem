using System;
using System.Collections.ObjectModel;
using System.Data;
using BarkodListem.Models;
using BarkodListem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BarkodListem.ViewModels;

public partial class IrsaliyeListViewModel : ObservableObject
{
    private readonly WebService _webService;

    [ObservableProperty]
    private DateTime baslangicTarihi = DateTime.Today.AddDays(-7);

    [ObservableProperty]
    private DateTime bitisTarihi = DateTime.Today;

    [ObservableProperty]
    private ObservableCollection<IrsaliyeModel> irsaliyeler = new();

    public IAsyncRelayCommand SorgulaCommand { get; }

    public IrsaliyeListViewModel(WebService webService)
    {
        _webService = webService;
        SorgulaCommand = new AsyncRelayCommand(SorgulaAsync);
    }

    private async Task SorgulaAsync()
    {
        var dt = await _webService.IrsaliyeSorgula(BaslangicTarihi, BitisTarihi);
        Irsaliyeler.Clear();
        foreach (DataRow row in dt.Rows)
        {
            var model = new IrsaliyeModel();
            if (dt.Columns.Contains("IRSALIYE_NO"))
                model.IRSALIYE_NO = row["IRSALIYE_NO"].ToString();
            if (dt.Columns.Contains("CARI_ADI"))
                model.CARI_ADI = row["CARI_ADI"].ToString();
            if (dt.Columns.Contains("TARIH"))
            {
                DateTime.TryParse(row["TARIH"].ToString(), out var tarih);
                model.TARIH = tarih;
            }
            Irsaliyeler.Add(model);
        }
    }
}