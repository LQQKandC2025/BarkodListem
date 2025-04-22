using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BarkodListem.Models;
using BarkodListem.Services;
using BarkodListem.Data;

namespace BarkodListem.ViewModels;

public partial class GaleriViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<GaleriResim> resimler = new();

    public async void Yukle(int stokId, string sevkiyatNo)
    {
        var db = await DatabaseService.GetConnectionAsync();

        var list = await db.Table<ResimModel>()
            .Where(r => r.RESIM_SAHIP_ID == stokId.ToString() && r.SEVKIYAT_NO == sevkiyatNo)
            .ToListAsync();

        Resimler.Clear();

        foreach (var item in list)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "Resimler", item.RESIM_ADI);
            Resimler.Add(new GaleriResim
            {
                ResimPath = path,
                OrjinalKayit = item
            });
        }
    }

    [RelayCommand]
    private async Task Sil(GaleriResim galeriResim)
    {
        if (galeriResim == null) return;

        // 1. Dosyadan sil
        if (File.Exists(galeriResim.ResimPath))
            File.Delete(galeriResim.ResimPath);

        // 2. DB'den sil
        var db = await DatabaseService.GetConnectionAsync();
        await db.DeleteAsync(galeriResim.OrjinalKayit);

        Resimler.Remove(galeriResim);
        await Shell.Current.DisplayAlert("Silindi", "Resim başarıyla silindi.", "Tamam");
    }
}

public class GaleriResim
{
    public string ResimPath { get; set; }
    public ResimModel OrjinalKayit { get; set; }
}
