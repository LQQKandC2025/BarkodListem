using BarkodListem.Data;
using BarkodListem.Models;
using BarkodListem.Pages;
using BarkodListem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace BarkodListem.ViewModels;
public partial class SSHFormViewModel : ObservableObject
{
    private readonly UrunModel _urun;
    private readonly string _sevkiyatNo;
    private readonly string _subeKodu;
    private readonly int _sirano;
    public decimal Miktar { get; set; }
    public int StokId { get; set; }
    public int SpecId { get; set; }
    public int SevkFisId { get; set; }
    public string SevkiyatNo { get; set; }
    public DateOnly Tarih { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string SshTuru { get; set; } = "ÜCRETSİZ";
    public string SorunDurum { get; set; } = "İŞLEMDE";
    public TimeOnly SshSure { get; set; } = new(0, 10);
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
        var mevcutAna = await db.Table<SSHAnaModel>()
            .Where(x => x.SEVKIYAT_NO == _sevkiyatNo)
            .FirstOrDefaultAsync();
        if (mevcutAna == null)
        {
            var yeniAna = new SSHAnaModel
            {
                TARIH = DateTime.Today,
                SUBE_KODU = _subeKodu,
                ACIKLAMA = SorunAciklama,
                CARI_ID = 0,
                DURUMU = "BEKLEMEDE",
                SEVKIYAT_NO = _sevkiyatNo
            };
            await db.InsertAsync(yeniAna);
        }
        var mevcutDetay = await db.Table<SSHDetayModel>()
            .Where(x => x.SEVKIYAT_NO == _sevkiyatNo && x.SIRANO == _sirano)
            .FirstOrDefaultAsync();
        if (mevcutDetay != null)
        {
            mevcutDetay.SORUN_ACIKLAMA = SorunAciklama;
            mevcutDetay.TARIH = DateTime.Today;
            mevcutDetay.MIKTAR = Convert.ToDouble(Miktar);
            mevcutDetay.SPEC_ID = SpecId;
            mevcutDetay.STOK_ID = StokId;
            mevcutDetay.SEVK_FIS_ID = SevkFisId;
            mevcutDetay.SEVKIYAT_NO = SevkiyatNo;
            await db.UpdateAsync(mevcutDetay);
        }
        else
        {
            var yeniDetay = new SSHDetayModel
            {
                SIRANO = _sirano,
                TARIH = DateTime.Today,
                SORUN_ACIKLAMA = SorunAciklama,
                MIKTAR = Convert.ToDouble(Miktar),
                STOK_ID = StokId,
                SPEC_ID = SpecId,
                SEVK_FIS_ID = SevkFisId,
                SSH_TURU = SshTuru,
                SORUN_DURUM = SorunDurum,
                SSH_SURE = DateTime.Today.Add(SshSure.ToTimeSpan()),
                TUTAR = 0,
                SEVKIYAT_NO = _sevkiyatNo
            };
            await db.InsertAsync(yeniDetay);
        }
        _urun.SORUNVAR = true;
        await Application.Current.MainPage.DisplayAlert("Başarılı", "SSH kaydı işlendi.", "Tamam");
        if (Application.Current.MainPage.Navigation.NavigationStack.FirstOrDefault(p => p is Pages.UrunListesiPage) is Pages.UrunListesiPage urunListesiPage
    && urunListesiPage.BindingContext is ViewModels.UrunListesiViewModel viewModel)
        {
            viewModel.RefreshUrun(_urun); // ✅ doğru ürünle tetikle
        }
    }
    [RelayCommand]
    private async Task ResimCek()
    {
        string? fullPath = null;
        var localRecordInserted = false;
        try
        {
            if (!int.TryParse(_urun.SIP_STR_ID, out var sipStrId))
                throw new InvalidOperationException("Ürün satır kimliği geçersiz; fotoğraf kaydedilemedi.");

            var result = await MediaPicker.CapturePhotoAsync();
            if (result == null) return;
            var klasorYolu = Path.Combine(FileSystem.AppDataDirectory, "Resimler", _sevkiyatNo);
            Directory.CreateDirectory(klasorYolu);
            var fileName = $"SSH_{SevkFisId}_{StokId}_{_urun.SIP_STR_ID}_{Guid.NewGuid():N}.jpg";
            fullPath = Path.Combine(klasorYolu, fileName);
            await PhotoStorageService.SaveOptimizedJpegAsync(result, fullPath);
            var db = await DatabaseService.GetConnectionAsync();
            var model = new ResimModel
            {
                SEVKIYAT_NO = _sevkiyatNo,
                STOK_ID= StokId,
                SIPSTR_ID = sipStrId,
                DOSYA_ADI = fileName,
                YOL="",
                TARIH = DateTime.Now,
                TIPI = "SSH"
            };
            await db.InsertAsync(model);
            localRecordInserted = true;
            await Application.Current.MainPage.DisplayAlert("Resim", $"Resim kaydedildi:\n{fileName}", "Tamam");
        }
        catch (Exception ex)
        {
            if (!localRecordInserted && fullPath != null && File.Exists(fullPath))
                File.Delete(fullPath);
            await Application.Current.MainPage.DisplayAlert("HATA", ex.Message, "Tamam");
        }
    }
    [RelayCommand]
    private async Task Galeri()
    {
        if (!int.TryParse(_urun.SIP_STR_ID, out var sipStrId))
        {
            await Application.Current.MainPage.DisplayAlert("HATA", "Ürün satır kimliği geçersiz.", "Tamam");
            return;
        }
        await Application.Current.MainPage.Navigation.PushAsync(new GaleriPage(StokId, _sevkiyatNo, sipStrId));
    }
}
