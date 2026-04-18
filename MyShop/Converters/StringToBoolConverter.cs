namespace MyShop.Converters;

/// <summary>
/// Converts "Active"/"Inactive" string to bool and back
/// </summary>
public class StringToBoolConverter : Microsoft.UI.Xaml.Data.IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string status)
        {
            return status.Equals("Active", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isOn)
        {
            return isOn ? "Active" : "Inactive";
        }
        return "Inactive";
    }
}
