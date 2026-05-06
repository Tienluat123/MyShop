using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public bool Inverse { get; set; }

    public object Convert(object value, System.Type targetType, object parameter, string language)
    {
        bool b = value is bool && (bool)value;
        if (Inverse) b = !b;
        
        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, string language)
    {
        if (value is Visibility v)
        {
            bool b = v == Visibility.Visible;
            return Inverse ? !b : b;
        }
        return false;
    }
}
