using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class NumberToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal d && d > 0)
            return Visibility.Visible;
        if (value is int i && i > 0)
            return Visibility.Visible;
        if (value is double db && db > 0)
            return Visibility.Visible;

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
