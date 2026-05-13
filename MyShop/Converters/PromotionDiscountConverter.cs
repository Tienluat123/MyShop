using System;
using Microsoft.UI.Xaml.Data;
using MyShop.Models;

namespace MyShop.Converters;

public class PromotionDiscountConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Promotion promotion)
        {
            return "Discount: 0 VND off";
        }

        var isPercent = promotion.PromoType.Equals("Percent", StringComparison.OrdinalIgnoreCase);
        var discount = isPercent
            ? $"{promotion.DiscountValue:N0}%"
            : $"{promotion.DiscountValue:N0} VND";

        return $"Discount: {discount} off";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
