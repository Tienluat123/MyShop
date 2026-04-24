using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class ColumnWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double actualWidth && actualWidth > 0)
        {
            if (!double.TryParse(parameter?.ToString(), out var columns) || columns <= 0)
            {
                return 200d;
            }

            // Subtract padding (24 left + 24 right = 48)
            double availableWidth = actualWidth - 48;
            return availableWidth / columns;
        }
        return 200d; // Fallback
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
