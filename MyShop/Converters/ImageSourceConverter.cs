using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace MyShop.Converters;

/// <summary>
/// Converter để tối ưu hóa việc tải ảnh.
/// Sử dụng DecodePixelWidth để giảm lượng RAM tiêu thụ và tăng tốc độ hiển thị.
/// </summary>
public class ImageSourceConverter : IValueConverter
{
    public int DecodeWidth { get; set; } = 60;

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        string? path = value as string;
        if (string.IsNullOrWhiteSpace(path)) return null;

        try
        {
            var uri = new Uri(path, UriKind.RelativeOrAbsolute);
            
            var bitmap = new BitmapImage();
            bitmap.DecodePixelWidth = DecodeWidth; 
            bitmap.DecodePixelType = DecodePixelType.Physical;
            bitmap.UriSource = uri;
            
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
}
