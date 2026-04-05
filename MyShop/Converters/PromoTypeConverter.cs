using System;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class PromoTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string s && s == "Percent") return "%";
        return " off";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
