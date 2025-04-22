using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BarkodListem.Models;
using BarkodListem.Services;
using BarkodListem.Data;
using System.IO;

namespace BarkodListem.ViewModels;

public partial class GaleriViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<GaleriResim> resimler = new();

    public async void Yukle(int stokId, string sevkiyatNo)
    {
        var db = await DatabaseService.GetConnectionAsync();
        string stokIdStr = stokId.ToString();

        var list = await db.Table<ResimModel>()
            .Where(r => r.RESIM_SAHIP_ID == stokIdStr &&
                        r.SEVKIYAT_NO == sevkiyatNo &&
                        r.RESIM_SAHIP == "SSH")
            .ToListAsync();

        Resimler.Clear();

        foreach (var item in list)
        {
            // 🔍 Doğru klasör yolu: Resimler/SEVKIYAT_NO/RESIM_ADI
            var path = Path.Combine(FileSystem.AppDataDirectory, "Resimler", item.SEVKIYAT_NO, item.RESIM_ADI);
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

        try
        {
            // 📁 Dosya sil
            if (File.Exists(galeriResim.ResimPath))
                File.Delete(galeriResim.ResimPath);

            // 🗃️ Veritabanından sil
            var db = await DatabaseService.GetConnectionAsync();
            await db.DeleteAsync(galeriResim.OrjinalKayit);

            // 🗑️ Listeden çıkar
            Resimler.Remove(galeriResim);

            await Application.Current.MainPage.DisplayAlert("Silindi", "Resim başarıyla silindi.", "Tamam");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("HATA", $"Silinemedi: {ex.Message}", "Tamam");
        }
    }
}

public class GaleriResim
{
    public string ResimPath { get; set; }
    public ResimModel OrjinalKayit { get; set; }
    public string ResimAdi => OrjinalKayit?.RESIM_ADI;
}
