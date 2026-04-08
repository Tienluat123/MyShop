using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MyShop.ViewModels;

namespace MyShop.Converters;

/// <summary>
/// Converter để hiển thị màu badge theo trạng thái khuyến mãi:
/// - Active: xanh lá
/// - Upcoming: xanh dương
/// - Expired: đỏ
/// - Inactive: xám
/// </summary>
public class PromoStatusBrushConverter : IValueConverter
{
    public bool IsBackground { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not PromotionItem promo)
            return new SolidColorBrush(Colors.Gray);

        if (!promo.IsActive)
        {
            // Inactive - xám
            return IsBackground
                ? new SolidColorBrush(ColorHelper.FromUint(0xFFF3F4F6))
                : new SolidColorBrush(ColorHelper.FromUint(0xFF6B7280));
        }

        if (promo.IsExpired)
        {
            // Expired - đỏ
            return IsBackground
                ? new SolidColorBrush(ColorHelper.FromUint(0xFFFEE2E2))
                : new SolidColorBrush(ColorHelper.FromUint(0xFFDC2626));
        }

        if (promo.IsUpcoming)
        {
            // Upcoming - xanh dương
            return IsBackground
                ? new SolidColorBrush(ColorHelper.FromUint(0xFFDBEAFE))
                : new SolidColorBrush(ColorHelper.FromUint(0xFF2563EB));
        }

        // Active - xanh lá
        return IsBackground
            ? new SolidColorBrush(ColorHelper.FromUint(0xFFD1FAE5))
            : new SolidColorBrush(ColorHelper.FromUint(0xFF059669));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Converter hiển thị text trạng thái khuyến mãi
/// </summary>
public class PromoStatusTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not PromotionItem promo)
            return "Unknown";

        return promo.StatusText;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Converter ẩn/hiện error message
/// </summary>
public class StringNullOrEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var str = value as string;
        return string.IsNullOrWhiteSpace(str)
            ? Microsoft.UI.Xaml.Visibility.Collapsed
            : Microsoft.UI.Xaml.Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}