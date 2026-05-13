using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string FormatWithParameter(string formatted)
        {
            return parameter is string format && !string.IsNullOrWhiteSpace(format)
                ? string.Format(CultureInfo.InvariantCulture, format, formatted)
                : formatted;
        }

        if (value is double d)
        {
            // Vietnamese Dong format: 1,000,000 VND
            return FormatWithParameter($"{d:N0} VND");
        }

        if (value is decimal dec)
        {
            return FormatWithParameter($"{dec:N0} VND");
        }

        if (value is int i)
        {
            return FormatWithParameter($"{i:N0} VND");
        }

        if (value is long l)
        {
            return FormatWithParameter($"{l:N0} VND");
        }

        return FormatWithParameter("0 VND");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
