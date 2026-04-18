using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
        {
            // Vietnamese Dong format: 1,000,000đ
            return $"{d:N0}đ";
        }

        if (value is decimal dec)
        {
            return $"{dec:N0}đ";
        }

        if (value is int i)
        {
            return $"{i:N0}đ";
        }

        if (value is long l)
        {
            return $"{l:N0}đ";
        }

        return "0đ";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
