using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace MyShop.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string status = value?.ToString() ?? "";

        return status switch
        {

            "Completed" => new SolidColorBrush(ColorHelper.ToColor("#D4EDDA")),
            "Cancelled" => new SolidColorBrush(ColorHelper.ToColor("#F8D7DA")),
            _ => new SolidColorBrush(Colors.Transparent) // Mặc định trong suốt
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}