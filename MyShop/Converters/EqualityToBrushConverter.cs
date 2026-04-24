using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace MyShop.Converters;

public class EqualityToBrushConverter : IValueConverter
{
    public Brush TrueBrush { get; set; } = new SolidColorBrush(Colors.Transparent);
    public Brush FalseBrush { get; set; } = new SolidColorBrush(Colors.Transparent);

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isEqual = value?.ToString() == parameter?.ToString();
        return isEqual ? TrueBrush : FalseBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
