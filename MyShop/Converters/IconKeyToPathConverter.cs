using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class IconKeyToPathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string key && Application.Current.Resources.TryGetValue(key, out var path))
        {
            return path as string ?? "";
        }
        return ""; // Fallback to empty path
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
