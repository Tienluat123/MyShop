using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyShop.Converters;

/// <summary>
/// Converter để tối ưu hóa việc tải ảnh.
/// Sử dụng DecodePixelWidth và weak cache để giảm RAM/CPU khi ảnh xuất hiện lặp lại.
/// </summary>
public class ImageSourceConverter : IValueConverter
{
    private static readonly Dictionary<string, WeakReference<BitmapImage>> BitmapCache = new();

    public int DecodeWidth { get; set; } = 180;

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        string? path = value as string;
        if (string.IsNullOrWhiteSpace(path)) return null;

        try
        {
            var normalizedPath = path.Trim();
            var decodeWidth = GetDecodeWidth(parameter);
            var cacheKey = $"{normalizedPath}|{decodeWidth}";

            if (BitmapCache.TryGetValue(cacheKey, out var cachedReference) &&
                cachedReference.TryGetTarget(out var cachedBitmap))
            {
                return cachedBitmap;
            }

            var bitmap = CreateBitmap(normalizedPath, decodeWidth);
            BitmapCache[cacheKey] = new WeakReference<BitmapImage>(bitmap);
            PruneDeadCacheEntries();

            return bitmap;
        }
        catch (Exception)
        {
            // Trả về null nếu path không hợp lệ
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    private int GetDecodeWidth(object parameter)
    {
        if (parameter is int intWidth && intWidth > 0)
        {
            return intWidth;
        }

        if (parameter is string stringWidth &&
            int.TryParse(stringWidth, out var parsedWidth) &&
            parsedWidth > 0)
        {
            return parsedWidth;
        }

        return DecodeWidth;
    }

    private static BitmapImage CreateBitmap(string path, int decodeWidth)
    {
        var bitmap = new BitmapImage
        {
            DecodePixelWidth = decodeWidth,
            DecodePixelType = DecodePixelType.Physical,
            UriSource = new Uri(path, UriKind.RelativeOrAbsolute)
        };

        return bitmap;
    }

    private static void PruneDeadCacheEntries()
    {
        if (BitmapCache.Count < 128)
        {
            return;
        }

        foreach (var key in BitmapCache.Keys.ToList())
        {
            if (!BitmapCache[key].TryGetTarget(out _))
            {
                BitmapCache.Remove(key);
            }
        }
    }
}
