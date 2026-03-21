using System;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class StringToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string s && double.TryParse(s.TrimEnd('%'), out var result))
        {
            return result;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return $"{value}%";
    }
}
