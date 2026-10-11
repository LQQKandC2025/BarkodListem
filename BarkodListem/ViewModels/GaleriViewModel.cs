using BarkodListem.Data;
using BarkodListem.Helpers;
using BarkodListem.Models;
using BarkodListem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
namespace BarkodListem.ViewModels;
public partial class GaleriViewModel : ObservableObject
{
    private readonly WebService _webService;

    public GaleriViewModel()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "barkodlistem.db");
        _webService = new WebService(new DatabaseService(dbPath));
    }

    [ObservableProperty]
    private ObservableCollection<GaleriResim> resimler = new();
    public async void Yukle(int stokId, string sevkiyatNo, string turu, int? sipStrId = null)
    {
        try
        {
            var db = await DatabaseService.GetConnectionAsync();
            var list = new List<ResimModel>();
            if (turu == "TESLİMAT")
            {
                list = await db.Table<ResimModel>()
                    .Where(r => r.SEVKIYAT_NO == sevkiyatNo && r.TIPI == turu)
                    .ToListAsync();
            }
            else if (turu == "SSH")
            {
                if (!sipStrId.HasValue)
                    throw new InvalidOperationException("SSH ürün satır kimliği bulunamadı.");
                list = await db.Table<ResimModel>()
                    .Where(r => r.STOK_ID == stokId &&
                                r.SEVKIYAT_NO == sevkiyatNo &&
                                r.SIPSTR_ID == sipStrId.Value &&
                                r.TIPI == turu)
                    .ToListAsync();
            }

            Resimler.Clear();
            var indirilemeyen = 0;
            foreach (var item in list)
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, "Resimler", item.SEVKIYAT_NO, item.DOSYA_ADI);
                if (!File.Exists(path) && item.GONDERIM == 1)
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                        var bytes = await _webService.ResimIndir(item.DOSYA_ADI);
                        var temporaryPath = path + ".download";
                        await File.WriteAllBytesAsync(temporaryPath, bytes);
                        File.Move(temporaryPath, path, true);
                    }
                    catch
                    {
                        indirilemeyen++;
                    }
                }

                if (File.Exists(path))
                {
                    Resimler.Add(new GaleriResim
                    {
                        ResimPath = path,
                        OrjinalKayit = item
                    });
                }
            }

            if (indirilemeyen > 0)
                await Application.Current.MainPage.DisplayAlert("Uyarı", $"{indirilemeyen} fotoğraf sunucudan indirilemedi.", "Tamam");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("HATA", "Galeri yüklenemedi: " + ex.Message, "Tamam");
        }
    }
    [RelayCommand]
    private async Task Sil(GaleriResim galeriResim)
    {
        if (galeriResim == null) return;
        try
        {
            if (galeriResim.OrjinalKayit.GONDERIM == 1)
            {
                var response = await _webService.ResimSil(galeriResim.OrjinalKayit);
                if (!SoapHelper.soap_json_basarili(response, "ResimSilResult", out var mesaj))
                    throw new InvalidOperationException(string.IsNullOrWhiteSpace(mesaj) ? "Sunucudaki resim silinemedi." : mesaj);
            }

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
