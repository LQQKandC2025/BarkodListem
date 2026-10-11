namespace BarkodListem.Services;

public static class PhotoStorageService
{
    private const int MaxDimension = 1920;
    private const int JpegQuality = 82;

    public static async Task SaveOptimizedJpegAsync(FileResult photo, string destinationPath)
    {
        var temporaryPath = destinationPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
#if ANDROID
        var boundsOptions = new Android.Graphics.BitmapFactory.Options
        {
            InJustDecodeBounds = true
        };

        await using (var boundsStream = await photo.OpenReadAsync())
        {
            Android.Graphics.BitmapFactory.DecodeStream(boundsStream, null, boundsOptions);
        }

        var sampleSize = 1;
        while (Math.Max(boundsOptions.OutWidth, boundsOptions.OutHeight) / (sampleSize * 2) >= MaxDimension)
        {
            sampleSize *= 2;
        }

        var rotation = 0;
        await using (var exifStream = await photo.OpenReadAsync())
        {
            using var exif = new Android.Media.ExifInterface(exifStream);
            rotation = exif.GetAttributeInt(Android.Media.ExifInterface.TagOrientation,
                (int)Android.Media.Orientation.Normal) switch
            {
                (int)Android.Media.Orientation.Rotate90 => 90,
                (int)Android.Media.Orientation.Rotate180 => 180,
                (int)Android.Media.Orientation.Rotate270 => 270,
                _ => 0
            };
        }

        var decodeOptions = new Android.Graphics.BitmapFactory.Options
        {
            InSampleSize = sampleSize,
            InPreferredConfig = Android.Graphics.Bitmap.Config.Argb8888
        };

        await using var sourceStream = await photo.OpenReadAsync();
        using var decoded = Android.Graphics.BitmapFactory.DecodeStream(sourceStream, null, decodeOptions)
                            ?? throw new InvalidOperationException("Fotoğraf dosyası okunamadı.");

        using var rotationMatrix = new Android.Graphics.Matrix();
        if (rotation != 0)
            rotationMatrix.PostRotate(rotation);
        using var oriented = rotation == 0
            ? null
            : Android.Graphics.Bitmap.CreateBitmap(decoded, 0, 0, decoded.Width, decoded.Height, rotationMatrix, true);
        var sourceBitmap = oriented ?? decoded;

        var ratio = Math.Min(1d, (double)MaxDimension / Math.Max(sourceBitmap.Width, sourceBitmap.Height));
        var targetWidth = Math.Max(1, (int)Math.Round(sourceBitmap.Width * ratio));
        var targetHeight = Math.Max(1, (int)Math.Round(sourceBitmap.Height * ratio));

        using var resized = ratio < 1d
            ? Android.Graphics.Bitmap.CreateScaledBitmap(sourceBitmap, targetWidth, targetHeight, true)
            : null;
        var outputBitmap = resized ?? sourceBitmap;

        await using (var output = File.Create(temporaryPath))
        {
            if (!outputBitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, JpegQuality, output))
                throw new IOException("Fotoğraf JPEG olarak kaydedilemedi.");
        }
#else
        await using var source = await photo.OpenReadAsync();
        await using (var output = File.Create(temporaryPath))
        {
            await source.CopyToAsync(output);
        }
#endif
            File.Move(temporaryPath, destinationPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }
}
