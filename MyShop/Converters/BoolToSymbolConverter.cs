using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Converters;

public class BoolToSymbolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isVisible && isVisible)
        {
            return Symbol.View; // Hình con mắt
        }
        return Symbol.Permissions; // Hình cái chìa khóa hoặc quyền truy cập
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
