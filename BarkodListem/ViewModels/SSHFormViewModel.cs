using System.Windows.Input;
using BarkodListem.Data;
using BarkodListem.Models;
using BarkodListem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Media;

namespace BarkodListem.ViewModels;

public partial class SSHFormViewModel : ObservableObject
{
    private readonly UrunModel _urun;
    private readonly string _sevkiyatNo;
    private readonly string _subeKodu;

    [ObservableProperty]
    private string sorunAciklama;

    public SSHFormViewModel(UrunModel urun, string sevkiyatNo, string subeKodu)
    {
        _urun = urun;
        _sevkiyatNo = sevkiyatNo;
        _subeKodu = subeKodu;
    }

    [RelayCommand]
    private async Task Kaydet()
    {
        var db = await DatabaseService.GetConnectionAsync();

        // 1. SSH_ANA kaydı
        var ana = new SSHAnaModel
        {
            TARIH = DateTime.Today,
            SUBE_KODU = _subeKodu,
            ACIKLAMA = SorunAciklama,
            DURUMU = "BEKLEMEDE",
            SEVKIYAT_NO = _sevkiyatNo
        };

        await db.InsertAsync(ana);

        // 2. SSH_DETAY kaydı
        var detay = new SSHDetayModel
        {
            TARIH = DateTime.Today,
            SORUN_ACIKLAMA = SorunAciklama,
            MIKTAR = Convert.ToDouble(_urun.MIKTAR),
            STOK_ID = Convert.ToInt32(_urun.STOK_ID),
            SPEC_ID = Convert.ToInt32(_urun.STOK_SPEC),
            SEVK_FIS_ID = Convert.ToInt32(_urun.SEVK_FIS_ID),
            SEVKIYAT_NO = _sevkiyatNo
        };

        await db.InsertAsync(detay);

        await Shell.Current.DisplayAlert("Başarılı", "SSH kaydı eklendi", "Tamam");
    }

    [RelayCommand]
    private async Task ResimCek()
    {
        try
        {
            var result = await MediaPicker.CapturePhotoAsync();

            if (result != null)
            {
                var fileName = Path.GetFileName(result.FullPath);
                var destDir = Path.Combine(FileSystem.AppDataDirectory, "Resimler");
                Directory.CreateDirectory(destDir);

                var newPath = Path.Combine(destDir, fileName);
                using var stream = await result.OpenReadAsync();
                using var newFile = File.OpenWrite(newPath);
                await stream.CopyToAsync(newFile);

                // RESIMLER tablosuna kayıt
                var db = await DatabaseService.GetConnectionAsync();
                var model = new ResimModel
                {
                    RESIM_SAHIP_ID = _urun.STOK_ID.ToString(),
                    RESIM_SAHIP = "SSH",
                    RESIM_ADI = fileName,
                    SEVKIYAT_NO = _sevkiyatNo
                };
                await db.InsertAsync(model);
                await Shell.Current.DisplayAlert("Resim", "Resim kaydedildi", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    [RelayCommand]
    private async Task Galeri()
    {
        await Shell.Current.GoToAsync($"galerisayfasi?stokId={_urun.STOK_ID}&sevkiyatNo={_sevkiyatNo}");
    }
}
