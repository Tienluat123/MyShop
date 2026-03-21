using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace MyShop.Converters;

public class CategoryBrushConverter : IValueConverter
{
    public object? ActiveBrush { get; set; }
    public object? InactiveBrush { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isActive = value is bool b && b;
        object? brushSource = isActive ? ActiveBrush : InactiveBrush;

        if (brushSource is Brush brush) return brush;
        if (brushSource is string hex) return new SolidColorBrush(ColorHelper.ToColor(hex));
        
        // Fallbacks
        return isActive 
            ? new SolidColorBrush(ColorHelper.ToColor("#D97706")) 
            : new SolidColorBrush(ColorHelper.ToColor("#9CA3AF"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public static class ColorHelper
{
    public static Windows.UI.Color FromUint(uint argb)
    {
        return Windows.UI.Color.FromArgb(
            (byte)((argb & 0xFF000000) >> 24), // Alpha
            (byte)((argb & 0x00FF0000) >> 16), // Red
            (byte)((argb & 0x0000FF00) >> 8),  // Green
            (byte)(argb & 0x000000FF)          // Blue
        );
    }

    public static Windows.UI.Color ToColor(string hex)
    {
        hex = hex.Replace("#", "");

        if (hex.Length == 8)
        {
            return FromUint(uint.Parse(hex, System.Globalization.NumberStyles.HexNumber));
        }

        byte a = 255;
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return Windows.UI.Color.FromArgb(a, r, g, b);
    }
}
