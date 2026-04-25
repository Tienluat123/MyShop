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
            double columns = double.Parse(parameter.ToString());
            // Subtract padding (24 left + 24 right = 48)
            double availableWidth = actualWidth - 48;
            return availableWidth / columns;
        }
        return 200; // Fallback
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
