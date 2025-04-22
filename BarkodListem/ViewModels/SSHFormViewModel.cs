using System.Windows.Input;
using BarkodListem.Data;
using BarkodListem.Models;
using BarkodListem.Pages;
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
    private readonly int _sirano;

    // Formda gösterilecek alanlar
    public decimal Miktar { get; set; }
    public int StokId { get; set; }
    public int SpecId { get; set; }
    public int SevkFisId { get; set; }
    public string SevkiyatNo { get; set; }
    public DateTime Tarih { get; set; } = DateTime.Today;
    public string SshTuru { get; set; } = "ÜCRETSİZ";
    public string SorunDurum { get; set; } = "İŞLEMDE";
    public DateTime SshSure { get; set; } = new(2010, 1, 1, 0, 10, 0);

    [ObservableProperty]
    private string sorunAciklama;

    public SSHFormViewModel(UrunModel urun, string sevkiyatNo, string subeKodu, int sirano)
    {
        _urun = urun;
        _sevkiyatNo = sevkiyatNo;
        _subeKodu = subeKodu;
        _sirano = sirano;

        Miktar = urun.MIKTAR;
        StokId = int.TryParse(urun.STOK_ID, out var st) ? st : 0;
        SpecId = int.TryParse(urun.STOK_SPEC, out var sp) ? sp : 0;
        SevkFisId = int.TryParse(urun.SEVK_FIS_ID, out var sf) ? sf : 0;
        SevkiyatNo = sevkiyatNo;

        _ = YukleKayitAsync();
    }

    private async Task YukleKayitAsync()
    {
        var db = await DatabaseService.GetConnectionAsync();
        var mevcut = await db.Table<SSHDetayModel>()
            .Where(x => x.SEVKIYAT_NO == _sevkiyatNo && x.SIRANO == _sirano)
            .FirstOrDefaultAsync();

        if (mevcut != null)
        {
            SorunAciklama = mevcut.SORUN_ACIKLAMA;
        }
    }

    [RelayCommand]
    private async Task Kaydet()
    {
        var db = await DatabaseService.GetConnectionAsync();

        var mevcut = await db.Table<SSHDetayModel>()
            .Where(x => x.SEVKIYAT_NO == _sevkiyatNo && x.SIRANO == _sirano)
            .FirstOrDefaultAsync();

        if (mevcut != null)
        {
            mevcut.SORUN_ACIKLAMA = SorunAciklama;
            await db.UpdateAsync(mevcut);
        }
        else
        {
            var detay = new SSHDetayModel
            {
                TARIH = DateTime.Today,
                SORUN_ACIKLAMA = SorunAciklama,
                MIKTAR = Convert.ToDouble(Miktar),
                STOK_ID = StokId,
                SPEC_ID = SpecId,
                SEVK_FIS_ID = SevkFisId,
                SEVKIYAT_NO = _sevkiyatNo,
                SIRANO = _sirano,
                SSH_TURU = SshTuru,
                SORUN_DURUM = SorunDurum,
                SSH_SURE = SshSure,
                TUTAR = 0,
                SSH_SEVK_FIS_ID = 0,
                IRS_STR_ID = 0,
                SORUN_KAYNAGI = "DİĞER",
                SORUN = "",
                SORUN_DETAY = ""
            };

            await db.InsertAsync(detay);
        }

        await Application.Current.MainPage.DisplayAlert("Başarılı", "SSH kaydı kaydedildi.", "Tamam");
    }

    [RelayCommand]
    private async Task ResimCek()
    {
        try
        {
            var result = await MediaPicker.CapturePhotoAsync();
            if (result == null) return;

            var fileName = Path.GetFileName(result.FullPath);
            var destDir = Path.Combine(FileSystem.AppDataDirectory, "Resimler");
            Directory.CreateDirectory(destDir);

            var newPath = Path.Combine(destDir, fileName);
            using var stream = await result.OpenReadAsync();
            using var newFile = File.OpenWrite(newPath);
            await stream.CopyToAsync(newFile);

            var db = await DatabaseService.GetConnectionAsync();
            var model = new ResimModel
            {
                RESIM_SAHIP_ID = StokId.ToString(),
                RESIM_SAHIP = "SSH",
                RESIM_ADI = fileName,
                SEVKIYAT_NO = _sevkiyatNo
            };
            await db.InsertAsync(model);
            await Application.Current.MainPage.DisplayAlert("Resim", "Resim kaydedildi", "Tamam");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("HATA", ex.Message, "Tamam");
        }
    }

    [RelayCommand]
    private async Task Galeri()
    {
        await Application.Current.MainPage.Navigation.PushAsync(new GaleriPage(StokId, _sevkiyatNo));
    }
}
