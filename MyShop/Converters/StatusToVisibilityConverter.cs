using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using MyShop.Models;

namespace MyShop.Converters;

public class StatusToVisibilityConverter : IValueConverter
{
    public string Target { get; set; } = "Active"; // "Active", "Cancel", "Refund"

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string statusStr = value?.ToString() ?? "";
        string target = (parameter as string) ?? Target;
        
        bool isVisible = target switch
        {
            "Active" => statusStr is "None" or "Preparing" or "Ready",
            "Cancel" => statusStr is "Draft" or "PendingPayment" or "Paid", 
            "Refund" => statusStr is "Paid",
            _ => false
        };

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
