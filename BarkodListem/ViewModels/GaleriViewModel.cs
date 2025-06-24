using BarkodListem.Data;
using BarkodListem.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
namespace BarkodListem.ViewModels;
public partial class GaleriViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<GaleriResim> resimler = new();
    public async void Yukle(int stokId, string sevkiyatNo, string turu)
    {
        var db = await DatabaseService.GetConnectionAsync();
        var list = new List<ResimModel>();
        if (turu == "TESLİMAT")
        {
            list = await db.Table<ResimModel>()
                .Where(r => r.SEVKIYAT_NO == sevkiyatNo &&
                            r.TIPI == turu)
                .ToListAsync();
        }
        else if (turu == "SSH")
        {
            string stokIdStr = stokId.ToString();
            int stokIdsi = int.Parse(UrunBilgi.STOK_ID);
            int sipstrId = int.Parse(UrunBilgi.SIP_STR_ID);
            list = await db.Table<ResimModel>()
                .Where(r => r.STOK_ID == stokIdsi &&
                            r.SEVKIYAT_NO == sevkiyatNo &&
                            r.SIPSTR_ID == sipstrId &&
                            r.TIPI == turu)
                .ToListAsync();
        }
        Resimler.Clear();
        foreach (var item in list)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "Resimler", item.SEVKIYAT_NO, item.DOSYA_ADI);
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
            if (File.Exists(galeriResim.ResimPath))
                File.Delete(galeriResim.ResimPath);
            var db = await DatabaseService.GetConnectionAsync();
            await db.DeleteAsync(galeriResim.OrjinalKayit);
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
    public string ResimAdi => OrjinalKayit?.DOSYA_ADI;
}
