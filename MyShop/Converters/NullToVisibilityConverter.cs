using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System.Collections;

namespace MyShop.Converters;

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isNull = value == null;
        
        if (value is ICollection collection)
        {
            isNull = collection.Count == 0;
        }

        bool inverse = parameter as string == "Inverse";
        bool isVisible = inverse ? isNull : !isNull;

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
