using System;
using Microsoft.UI.Xaml.Data;
using MyShop.Models;

namespace MyShop.Converters;

public class StatusToNextStatusTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string statusStr = value?.ToString() ?? "";

        return statusStr switch
        {
            "None" => "Update to Preparing",
            "Preparing" => "Update to Ready",
            "Ready" => "Update to Completed",
            _ => ""
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
