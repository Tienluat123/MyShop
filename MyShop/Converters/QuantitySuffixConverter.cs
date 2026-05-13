using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Converters;

public class QuantitySuffixConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null) return "0 items";
        return $"{value} items";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
