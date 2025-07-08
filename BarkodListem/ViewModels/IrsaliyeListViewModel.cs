using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BarkodListem.Models;

namespace BarkodListem.ViewModels;

public partial class IrsaliyeListViewModel : ObservableObject
{
    [ObservableProperty]
    private DateTime startDate = DateTime.Today.AddDays(-7);

    [ObservableProperty]
    private DateTime endDate = DateTime.Today;

    public ObservableCollection<IrsaliyeModel> Irsaliyeler { get; } = new();

    public IrsaliyeListViewModel()
    {
        SorgulaCommand = new AsyncRelayCommand(SorgulaAsync);
    }

    public IAsyncRelayCommand SorgulaCommand { get; }

    private async Task SorgulaAsync()
    {
        Irsaliyeler.Clear();
        await Task.Delay(100);
        for (int i = 1; i <= 5; i++)
        {
            Irsaliyeler.Add(new IrsaliyeModel
            {
                IRSALIYE_ID = i,
                IRSALIYE_TURU = $"TUR{i}",
                IRS_TARIH = StartDate.AddDays(i),
                IRS_NO = $"IR-{i:000}"
            });
        }
    }
}