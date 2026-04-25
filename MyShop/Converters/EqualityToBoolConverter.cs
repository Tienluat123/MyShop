using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class EqualityToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || parameter == null)
            return false;

        return value.ToString() == parameter.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b && b && parameter != null)
            return parameter.ToString()!;

        return DependencyProperty.UnsetValue;
    }
}
