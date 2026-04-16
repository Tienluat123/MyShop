using System;
using Microsoft.UI.Xaml.Data;
using MyShop.Models;

namespace MyShop.Converters;

public class StatusToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string statusStr = value?.ToString() ?? "";

        return statusStr switch
        {
            "None" or "Draft" or "Pending" => "&#xE819;",   // Clipboard
            "Preparing" or "PendingPayment" => "&#xE916;", // Timer/Recent
            "Ready" => "&#xE73E;",     // CheckMark
            "Completed" or "Paid" => "&#xE7BA;", // Home/Check
            "Cancelled" => "&#xE711;",    // Cancel
            "Refunded" => "&#xE777;",  // Refresh/Rotate
            _ => "&#xE819;"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
