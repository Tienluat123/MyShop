using System;
using Microsoft.UI.Xaml.Data;
using MyShop.Models;

namespace MyShop.Converters;

public class StatusToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not OrderStatus status) return "&#xE819;"; // Default Clipboard

        return status switch
        {
            OrderStatus.Pending => "&#xE819;",   // Clipboard
            OrderStatus.Preparing => "&#xE916;", // Timer/Recent
            OrderStatus.Ready => "&#xE73E;",     // CheckMark
            OrderStatus.Completed => "&#xE7BA;", // Home/Check
            OrderStatus.Cancelled => "&#xE711;",    // Cancel
            OrderStatus.Refunded => "&#xE777;",  // Refresh/Rotate
            _ => "&#xE819;"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
