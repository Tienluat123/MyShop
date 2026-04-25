using System;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class StringFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string format = parameter as string;
        if (string.IsNullOrEmpty(format)) return value?.ToString() ?? string.Empty;

        return string.Format(format, value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
