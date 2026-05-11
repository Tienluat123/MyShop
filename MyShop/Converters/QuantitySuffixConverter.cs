using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Converters;

public class QuantitySuffixConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null) return "0 món";
        return $"{value} món";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
