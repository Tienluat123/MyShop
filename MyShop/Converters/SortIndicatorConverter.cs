using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Converters;

public class SortIndicatorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value == null || parameter == null)
            return "\u00A0"; // Non-breaking space (empty)

        var sortInfo = value as string; // Format: "ColumnName|SortAscending"
        var columnParam = parameter as string;

        if (string.IsNullOrEmpty(sortInfo) || string.IsNullOrEmpty(columnParam))
            return "\u00A0";

        var parts = sortInfo.Split('|');
        if (parts.Length != 2)
            return "\u00A0";

        var currentColumn = parts[0];
        var isAscending = bool.TryParse(parts[1], out var asc) && asc;

        // Only show indicator if this column is being sorted
        if (currentColumn != columnParam)
            return "\u00A0";

        // Return appropriate icon based on sort direction
        // \u21BE = up arrow (ascending), \u21C2 = down arrow (descending)
        return isAscending ? "\u21BE" : "\u21C2";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}
