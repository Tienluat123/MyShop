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
        if (value is not OrderStatus status) return new SolidColorBrush(Colors.Gray);

        return status switch
        {
            OrderStatus.Pending => IsBackground ? new SolidColorBrush(ColorHelper.FromUint(0xFFF3F4F6)) : new SolidColorBrush(ColorHelper.FromUint(0xFF6B7280)),
            OrderStatus.Preparing => IsBackground ? new SolidColorBrush(ColorHelper.FromUint(0xFFFEF3C7)) : new SolidColorBrush(ColorHelper.FromUint(0xFFB45309)),
            OrderStatus.Ready => IsBackground ? new SolidColorBrush(ColorHelper.FromUint(0xFFD1FAE5)) : new SolidColorBrush(ColorHelper.FromUint(0xFF065F46)),
            OrderStatus.Completed => IsBackground ? new SolidColorBrush(ColorHelper.FromUint(0xFFDBEAFE)) : new SolidColorBrush(ColorHelper.FromUint(0xFF1E40AF)),
            OrderStatus.Cancelled => IsBackground ? new SolidColorBrush(ColorHelper.FromUint(0xFFFEE2E2)) : new SolidColorBrush(ColorHelper.FromUint(0xFF991B1B)),
            OrderStatus.Refunded => IsBackground ? new SolidColorBrush(ColorHelper.FromUint(0xFFF3E8FF)) : new SolidColorBrush(ColorHelper.FromUint(0xFF6B21A8)),
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
