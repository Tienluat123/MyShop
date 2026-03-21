using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using MyShop.Models;

namespace MyShop.Converters;

public class StatusToVisibilityConverter : IValueConverter
{
    public string Target { get; set; } = "Active"; // "Active", "Void", "Refund"

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not OrderStatus status) return Visibility.Collapsed;

        string target = (parameter as string) ?? Target;
        
        bool isVisible = target switch
        {
            "Active" => status == OrderStatus.Pending || status == OrderStatus.Preparing || status == OrderStatus.Ready,
            "Void" => status == OrderStatus.Pending || status == OrderStatus.Preparing || status == OrderStatus.Ready,
            "Refund" => status == OrderStatus.Completed,
            _ => false
        };

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
