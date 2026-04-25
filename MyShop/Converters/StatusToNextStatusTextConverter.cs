using System;
using Microsoft.UI.Xaml.Data;
using MyShop.Models;

namespace MyShop.Converters;

public class StatusToNextStatusTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not OrderStatus status) return "";

        return status switch
        {
            OrderStatus.Pending => "Update to Preparing",
            OrderStatus.Preparing => "Update to Ready",
            OrderStatus.Ready => "Update to Completed",
            _ => ""
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
