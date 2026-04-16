using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class RoleToDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string role)
        {
            return role == "1" ? "Admin" : "Sale";
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
