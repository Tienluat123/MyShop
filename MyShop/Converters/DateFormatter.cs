using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Converters;

public class DateFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dt)
        {
            return dt.ToString("dd/MM/yyyy HH:mm");
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
