using BarkodListem.Data;
using BarkodListem.Models;

namespace BarkodListem.Services;

public static class PhotoCacheService
{
    private const long MaximumCacheBytes = 250L * 1024 * 1024;
    private static readonly TimeSpan MaximumAge = TimeSpan.FromDays(90);

    public static async Task TrimUploadedCacheAsync()
    {
        var root = Path.Combine(FileSystem.AppDataDirectory, "Resimler");
        if (!Directory.Exists(root))
            return;

        var db = await DatabaseService.GetConnectionAsync();
        var records = await db.Table<ResimModel>().ToListAsync();
        var uploaded = records.Where(x => x.GONDERIM == 1)
                              .Select(x => CacheKey(x.SEVKIYAT_NO, x.DOSYA_ADI))
                              .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories)
                             .Select(path => new FileInfo(path))
                             .ToList();
        var cutoff = DateTime.Now - MaximumAge;

        foreach (var file in files.Where(x => x.LastWriteTime < cutoff))
        {
            var shipment = file.Directory?.Name ?? string.Empty;
            if (uploaded.Contains(CacheKey(shipment, file.Name)))
                TryDelete(file.FullName);
        }

        files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories)
                         .Select(path => new FileInfo(path))
                         .OrderBy(x => x.LastWriteTimeUtc)
                         .ToList();
        var totalBytes = files.Sum(x => x.Length);
        foreach (var file in files)
        {
            if (totalBytes <= MaximumCacheBytes)
                break;

            var shipment = file.Directory?.Name ?? string.Empty;
            if (!uploaded.Contains(CacheKey(shipment, file.Name)))
                continue;

            var length = file.Length;
            if (TryDelete(file.FullName))
                totalBytes -= length;
        }
    }

    public static async Task RemoveOrphanFilesAsync()
    {
        var root = Path.Combine(FileSystem.AppDataDirectory, "Resimler");
        if (!Directory.Exists(root))
            return;

        var db = await DatabaseService.GetConnectionAsync();
        var records = await db.Table<ResimModel>().ToListAsync();
        var known = records.Select(x => CacheKey(x.SEVKIYAT_NO, x.DOSYA_ADI))
                           .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var path in Directory.GetFiles(root, "*.*", SearchOption.AllDirectories))
        {
            var file = new FileInfo(path);
            var shipment = file.Directory?.Name ?? string.Empty;
            if (!known.Contains(CacheKey(shipment, file.Name)))
                TryDelete(path);
        }
    }

    private static string CacheKey(string shipment, string fileName) => shipment + "|" + fileName;

    private static bool TryDelete(string path)
    {
        try
        {
            File.Delete(path);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
