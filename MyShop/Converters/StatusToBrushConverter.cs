using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MyShop.Models;

namespace MyShop.Converters;

public class StatusToBrushConverter : IValueConverter
{
    public bool IsBackground { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string statusStr = value?.ToString() ?? "";

        return statusStr switch
        {
            "None" or "Pending" or "Draft" => IsBackground ? new SolidColorBrush(ColorHelper.FromUint(0xFFF3F4F6)) : new SolidColorBrush(ColorHelper.FromUint(0xFF6B7280)),
            "Preparing" or "PendingPayment" => IsBackground ? new SolidColorBrush(ColorHelper.FromUint(0xFFFEF3C7)) : new SolidColorBrush(ColorHelper.FromUint(0xFFB45309)),
            "Ready" => IsBackground ? new SolidColorBrush(ColorHelper.FromUint(0xFFD1FAE5)) : new SolidColorBrush(ColorHelper.FromUint(0xFF065F46)),
            "Completed" or "Paid" => IsBackground ? new SolidColorBrush(ColorHelper.FromUint(0xFFDBEAFE)) : new SolidColorBrush(ColorHelper.FromUint(0xFF1E40AF)),
            "Cancelled" => IsBackground ? new SolidColorBrush(ColorHelper.FromUint(0xFFFEE2E2)) : new SolidColorBrush(ColorHelper.FromUint(0xFF991B1B)),
            "Refunded" => IsBackground ? new SolidColorBrush(ColorHelper.FromUint(0xFFF3E8FF)) : new SolidColorBrush(ColorHelper.FromUint(0xFF6B21A8)),
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
